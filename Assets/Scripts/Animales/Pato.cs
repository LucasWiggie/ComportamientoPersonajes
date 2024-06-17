using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Cocodrilo;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;


public class Pato : MonoBehaviour
{
    public float radio;
    public float radioNenufar;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask targetMaskNenufar;
    public LayerMask obstructionMask;
    public LayerMask huirCocodrilo;


    public bool puedeVer;
    public bool puedeVerNenufar;

    //Acci�n ir a nenufares
    public enum ChaseState
    {
        Finished,
        Enproceso,
        Failed
    }

    //BTs
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedo;

    //bool Necesidades
    private bool boolHambre;
    private bool boolEnergia;
    private bool boolMiedo;

    //Utilidades
    public float uHambre;
    public float uEnergia;
    public float uMiedo;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    float hambreRate = 2f;
    float energiaRate = 2f;

    public bool isDefaultMov = true;

    public bool aSalvo = false;
    public Salamandra salamandraScript;
    public GameObject patito;
    private bool fertil;
    private bool descansando = false;

    //objetivos
    private Transform nenufarTarget;//huevos objetivo
    private Transform salTarget;

    //NavMeshAgent
    private NavMeshAgent patoNav;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 3f;

    //Presas indefensas
    private List<Collider> animalesNoASalvo = new List<Collider>();

    //Lista con los cocodrilos cercanos al pato
    private List<Transform> cocodrilosCercanos = new List<Transform>();
    public float distanciaMaxima = 1f;

    //Getters y Setters
    public float getHambre()
    {
        return this.hambre;
    }
    public float getEnergia()
    {
        return this.energia;
    }
    public float getMiedo()
    {
        return this.miedo;
    }
    public void setHambre(float h)
    {
        this.hambre = h;
    }
    public void setEnergia(float e)
    {
        this.energia = e;
    }
    public void setMiedo(float m)
    {
        this.miedo = m;
    }

    private void Start()
    {
        patoNav = GetComponent<NavMeshAgent>();
        playerRef = this.gameObject;

        hambre = Random.Range(15, 40);
        energia = Random.Range(80, 100);
        miedo = 0;

        uHambre = hambre;
        uEnergia = energia;
        uMiedo = miedo;
    }

    private void Update()
    {
        UpdateVariables();//actualizamos necesidades
        DetectarObjetivos();//detectamos cocodrilos
        ActualizarMiedo();//actualizamos miedo
    }

    private void FixedUpdate()
    {
        UtilitySystem();
        if (isDefaultMov)
        {
            movimientoAleatorio();
        }
        else if (boolHambre)
        {
            btHambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedo)
        {
            btMiedo.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolEnergia)
        {
            btEnergia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (fertil)
        {
            GenerarPatitos();
        }
    }

    public void UtilitySystem()
    {
        uHambre = this.getHambre();
        uEnergia = this.getEnergia();
        uMiedo = this.getMiedo();
        animalesNoASalvo = ObtenerAnimalesNoASalvo();

        if (uHambre >= 100 || uEnergia <= 0)
        {
            Debug.Log(this.gameObject + " ha muerto");
            patoNav = null;
            Destroy(this.gameObject);
        }
        else if (uMiedo > 90)//si miedo -> arbol de miedo
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            fertil = false;
            aSalvo = false;
            boolMiedo = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(true);
        }
        else if (uEnergia < 20 || descansando) //si energia baja o descansando -> arbol de energia
        {
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = false;
            fertil = false;
            aSalvo = false;
            boolEnergia = true;
            btHambre.SetActive(false);
            btMiedo.SetActive(false);
            btEnergia.SetActive(true);

        }
        else if (uHambre < 20 && uEnergia > 70) //si sufienciente energia y poca hambre -> generar patitos
        {
            boolEnergia = false;
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = false;
            aSalvo = false;
            fertil = true;
            btEnergia.SetActive(false);
            btHambre.SetActive(false);
            btMiedo.SetActive(false);
        }
        else if (uHambre > 80 && animalesNoASalvo.Count != 0)// si hambre -> arbol de hambre
        {
            boolEnergia = false;
            boolMiedo = false;
            isDefaultMov = false;
            fertil = false;
            boolHambre = true;
            aSalvo = false;
            btMiedo.SetActive(false);
            btEnergia.SetActive(false);
            btHambre.SetActive(true);
        }
        else if (uMiedo > 70)//si miedo -> arbol de miedo
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            fertil = false;
            aSalvo = false;
            boolMiedo = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(true);
        }
        else //si nada -> movimiento estandar
        {
            boolEnergia = false;
            boolHambre = false;
            boolMiedo = false;
            fertil = false;
            isDefaultMov = true;
            aSalvo = false;
            btEnergia.SetActive(false);
            btHambre.SetActive(false);
            btMiedo.SetActive(false);
        }
    }

    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);
    }

    private void movimientoAleatorio()
    {
        try
        {
            if (Time.time >= nextRandomMovementTime)
            {
                Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
                patoNav?.SetDestination(randomPoint); // Establecer el punto como destino
                                                     //Debug.Log("pato se mueve");
                nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el pr�ximo movimiento
            }
        }
        catch { }
    }


    // Funci�n para encontrar un punto aleatorio en el NavMesh dentro de un radio dado
    private Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }


    public ChaseState ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        //salamandras que no estan resguardadas
        List<Collider> salamandrasNoASalvo = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es una salamandra y si no est� a salvo
            Salamandra salamandra = targetParent.GetComponent<Salamandra>();

            if ((salamandra != null && !salamandra.aSalvo))
            {
                salamandrasNoASalvo.Add(col);
            }
        }

        // Verificar si hay objetivos no a salvo
        if (salamandrasNoASalvo.Count > 0)
        {
            Transform target = salamandrasNoASalvo[0].transform; // Utilizar el primer objetivo no a salvo encontrado
            salTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget); // Utilizar el producto punto para verificar el �ngulo
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2)); // Establecer un umbral para el �ngulo (ajustar seg�n sea necesario)

            if (dotProduct > angleThreshold)
            {
                float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                {
                    puedeVer = true;

                    return ChaseState.Finished;
                }
                else
                {
                    puedeVer = false;
                    return ChaseState.Failed;
                }
            }
            else
            {
                puedeVer = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVer)
        {
            puedeVer = false;
            return ChaseState.Failed;
        }
        return ChaseState.Failed;
    }

    //Acci�n Perseguir Salamandra
    public ChaseState PerseguirSal()
    {
        try
        {
            if (salTarget == null)
            {
                return ChaseState.Failed;
            }

            patoNav.speed = 4.75f;
            GameObject targetParent = salTarget.gameObject;
            var salamandra = targetParent.GetComponent<Salamandra>();

            if (salamandra == null)
            {
                return ChaseState.Failed;
            }

            if (salamandra.aSalvo)
            {
                patoNav.speed = 3.5f;
                return ChaseState.Failed;
            }

            float distanciaMinimaParaComer = 2.0f;
            float distanciaActual = Vector3.Distance(salTarget.position, transform.position);

            Debug.Log($"Chasing target. Current distance: {distanciaActual}, Minimum distance: {distanciaMinimaParaComer}");

            if (distanciaActual > distanciaMinimaParaComer)
            {
                patoNav?.SetDestination(salTarget.position);
                return ChaseState.Enproceso;
            }
            else
            {
                patoNav.speed = 3.5f;
                return ChaseState.Finished; // Suficientemente cerca para intentar comer
            }
        }
        catch { return ChaseState.Failed; } 
        
    }

    //Acci�n Comer Salamandra
    public void ComerSal()
    {
        // Verificar si salTarget no es null
        if (salTarget == null)
        {
            return;
        }

        GameObject targetParent = salTarget.gameObject;
        var salamandra = targetParent.GetComponent<Salamandra>();

        if (salamandra != null)
        {
            if (!salamandra.aSalvo)
            {
                float distanciaMinimaParaComer = 2.0f;
                float distanciaActual = Vector3.Distance(salTarget.position, transform.position);

                if (distanciaActual <= distanciaMinimaParaComer)
                {
                    GameObject.Destroy(targetParent); // Destruir el GameObject de la salamandra que se ha comido
                    hambre -= 50; // Bajar el hambre
                }

            }
        }
    }

    public ChaseState HayNenufares()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioNenufar, targetMaskNenufar);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // Verificar que el objetivo est� dentro del �ngulo de visi�n
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));
                if (dotProduct > angleThreshold)
                {
                    float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                    // Verificar que no haya obstrucciones entre el castor y la presa
                    if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                    {
                        // Si este objetivo est� m�s cerca que los anteriores, actualizar el m�s cercano
                        if (distanciaToTarget < closestDistance)
                        {
                            closestDistance = distanciaToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar la presa m�s cercana como el objetivo
                nenufarTarget = closestTarget;
                puedeVerNenufar = true;
                return ChaseState.Finished;
            }
            else
            {
                puedeVerNenufar = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVerNenufar)
        {
            puedeVerNenufar = false;
            return ChaseState.Failed;
        }

        return ChaseState.Failed;
    }

    public ChaseState IrNenufares()
    {
        try
        {
            if (nenufarTarget != null)
            {

                patoNav?.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar

                if ((transform.position.x - nenufarTarget.position.x <= 0.5f) && (transform.position.z - nenufarTarget.position.z <= 0.5F))
                {
                    Debug.Log("pato en nenufar");
                    energia += 0.4f;//vuelve a tener energia
                    aSalvo = true;
                    descansando = true;
                    if (energia > 90)//una cantidad necesaria de energia que reponer para poder salir del nenufar, evitando cambios de comportamiento por cte por el cambio del valor de energia en la franja de cansancio
                    {
                        descansando = false;
                    }
                    return ChaseState.Finished;
                }
                Debug.Log("en proceso");
                return ChaseState.Enproceso;


            }
            else
            {
                Debug.Log("nenufar null");
                return ChaseState.Failed;
            }
        } 
        catch (Exception e) { return ChaseState.Failed; }
        
    }

    public ChaseState HuirNenufares()
    {
        try
        {
            if (nenufarTarget != null)
            {

                patoNav?.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar
                patoNav.speed = patoNav.speed + 2f;
                energia -= 0.02f;
                energia = Mathf.Clamp(energia, 0f, 100f);

                if ((transform.position.x - nenufarTarget.position.x <= 0.5f) && (transform.position.z - nenufarTarget.position.z <= 0.5f))
                {
                    Debug.Log("pato en nenufar");
                    aSalvo = true;
                    patoNav.speed = patoNav.speed - 2f;
                    return ChaseState.Finished;
                }
                patoNav.speed = patoNav.speed - 2f;
                return ChaseState.Enproceso;



            }
            else
            {
                patoNav.stoppingDistance = 0;
                patoNav.speed--;
                return ChaseState.Failed;
            }
        }
        catch {  return ChaseState.Failed; }

        
    }


    public void GenerarPatitos()
    {
        energia -= 20;//resta energfia y aumenta hambre
        hambre += 10;
        Instantiate(patito, transform.position, transform.rotation); //se generarn patitos en la posicion actual


    }

    void DetectarObjetivos()
    {
        cocodrilosCercanos.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, distanciaMaxima, huirCocodrilo);
        foreach (Collider collider in colliders)
        {
            cocodrilosCercanos.Add(collider.transform);
        }
    }

    void ActualizarMiedo()
    {
        miedo = 0f;

        foreach (Transform objetivo in cocodrilosCercanos)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.position);
            float miedoIncremental = Mathf.Clamp01(1 - distancia / distanciaMaxima) * 100; // Calcular el miedo incremental normalizado

            miedo = Mathf.Max(miedo, miedoIncremental); // Mantener el mayor valor de miedo
        }
    }


    public List<Collider> ObtenerAnimalesNoASalvo()
    {
        // Obtener todos los colisionadores en el rango especificado
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        // Lista para almacenar colisionadores de animales no a salvo
        List<Collider> animalesNoASalvo = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es un castor o un pato y si no está a salvo
            Salamandra salamandra = targetParent.GetComponent<Salamandra>();

            if ((salamandra != null && !salamandra.aSalvo))
            {
                animalesNoASalvo.Add(col);
            }
        }

        return animalesNoASalvo;
    }
}

