using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Cocodrilo;
using static UnityEngine.GraphicsBuffer;

public class Pato : MonoBehaviour
{
    public float radio;
    public float radioNenufar;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public GameObject patito;


    public LayerMask targetMask;
    public LayerMask targetMaskNenufar;
    public LayerMask obstructionMask;
    public LayerMask huirCocodrilo;


    public bool puedeVer;
    public bool puedeVerNenufar;

    //Lista con los cocodrilos cercanos al pato
    private List<Transform> cocodrilosCercanos = new List<Transform>();
    public float distanciaMaxima = 1f;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    //Utilidades
    public float _hambre;
    public float _energia;
    public float _miedo;

    //BTs
    public GameObject BT_Hambre;
    public GameObject BT_Energia;
    public GameObject BT_Miedo;

    //bool Necesidades
    private bool bool_Hambre;
    private bool bool_Energia;
    private bool bool_Miedo;
    private bool fertil;
    private bool descansando = false;

    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    public bool aSalvo = false;
    //NavMeshAgent
    private NavMeshAgent patoNav;
    //objetivos
    private Transform nenufarTarget;//huevos objetivo
    private Transform salTarget;

    public bool isDefaultMov = true;
    private bool dirtyUS = false;

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

        hambre = 60;
        energia = 100;
        miedo = 0;

        _hambre = hambre;
        _energia = energia;
        _miedo = miedo;

        StartCoroutine(FOVRoutine());
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
            Debug.Log("Me muevo");
            movimientoAleatorio();
        }
        else if (bool_Hambre)
        {
            Debug.Log("Tengo Hambre");
            BT_Hambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (bool_Miedo)
        {
            Debug.Log("Tengo Miedo");
            BT_Miedo.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (bool_Energia)
        {
            Debug.Log("Tengo cansancio");
            BT_Energia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (fertil)
        {
            Debug.Log("Genero patitos");
            GenerarPatitos();
        }

    }

    public void UtilitySystem() 
    {
        _hambre = this.getHambre();
        _energia = this.getEnergia();
        _miedo = this.getMiedo();

        if (_energia < 50 || descansando) //si energia baja o descansando -> arbol de energia
        {
            bool_Hambre = false;
            bool_Miedo = false;
            isDefaultMov = false;
            fertil = false;
            aSalvo = false;
            bool_Energia = true;
            BT_Hambre.SetActive(false);
            BT_Miedo.SetActive(false);
            BT_Energia.SetActive(true);

        }else if (_hambre < 20 && _energia > 70) //si sufienciente energia y poca hambre -> generar patitos
        {
            bool_Energia = false;
            bool_Hambre = false;
            bool_Miedo = false;
            isDefaultMov = false;
            aSalvo = false;
            fertil = true;
            BT_Energia.SetActive(false);
            BT_Hambre.SetActive(false);
            BT_Miedo.SetActive(false);
        }
        else if (_miedo > 50)//si miedo -> arbol de miedo
        {
            bool_Energia = false;
            bool_Hambre = false;
            isDefaultMov = false;
            fertil = false;
            aSalvo = false;
            bool_Miedo = true;
            BT_Hambre.SetActive(false);
            BT_Energia.SetActive(false);
            BT_Miedo.SetActive(true);
        }
        else if (_hambre > 50)// si hambre -> arbol de hambre
        {
            bool_Energia = false;
            bool_Miedo = false;
            isDefaultMov = false;
            fertil = false;
            bool_Hambre = true;
            aSalvo = false;
            BT_Miedo.SetActive(false);
            BT_Energia.SetActive(false);
            BT_Hambre.SetActive(true);
        }
        else //si nada -> movimiento estandar
        {
            bool_Energia = false;
            bool_Hambre = false;
            bool_Miedo = false;
            fertil= false;
            isDefaultMov = true;
            aSalvo = false;
            BT_Energia.SetActive(false);
            BT_Hambre.SetActive(false);
            BT_Miedo.SetActive(false);
        }
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


    private void movimientoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            patoNav.SetDestination(randomPoint); // Establecer el punto como destino
            //Debug.Log("pato se mueve");
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el pr�ximo movimiento
        }
    }

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 3f;

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

    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);
    }


    private IEnumerator FOVRoutine()
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (true)
        {
            yield return wait;
            ComprobarVision();
        }
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
            // Utilizar el primer objetivo no a salvo encontrado
            Transform target = salamandrasNoASalvo[0].transform;
            salTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Utilizar el producto punto para verificar el �ngulo
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            // Establecer un umbral para el �ngulo (ajustar seg�n sea necesario)
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));
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

    //Acci�n ir a nenufares
    public enum ChaseState
    {
        Finished,
        Enproceso,
        Failed
    }


    public ChaseState IrNenufares()
    {
        
        if (nenufarTarget != null)
        {

            patoNav.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar

            if (transform.position.x == nenufarTarget.position.x && transform.position.z == nenufarTarget.position.z)
            {
                Debug.Log("pato en nenufar");
                energia += 0.08f;//vuelve a tener energia
                descansando = true;
                if (energia > 80)//una cantidad necesaria de energia que reponer para poder salir del nenufar, evitando cambios de comportamiento por cte por el cambio del valor de energia en la franja de cansancio
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

    public ChaseState HuirNenufares()
    {
        
        if (nenufarTarget != null)
        {
        
                patoNav.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar
                patoNav.speed = patoNav.speed + 2f;
                energia -= 2;
                energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == nenufarTarget.position.x && transform.position.z == nenufarTarget.position.z)
            {
                Debug.Log("pato en nenufar");
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

    //Acci�n Perseguir Salamandra
    public ChaseState PerseguirSal()
    {
        if (salTarget == null)
        {
            return ChaseState.Failed; 
        }

        GameObject targetParent = salTarget.gameObject;
        var salamandra = targetParent.GetComponent<Salamandra>();

        if (salamandra == null)
        {
            return ChaseState.Failed;
        }

        if (salamandra.aSalvo) 
        {
            return ChaseState.Finished;
        }

        float distanciaMinimaParaComer = 2.0f; 
        float distanciaActual = Vector3.Distance(salTarget.position, transform.position);

        Debug.Log($"Chasing target. Current distance: {distanciaActual}, Minimum distance: {distanciaMinimaParaComer}");

        if (distanciaActual > distanciaMinimaParaComer)
        {
            patoNav.SetDestination(salTarget.position); 
            return ChaseState.Enproceso; 
        }
        else
        {
            return ChaseState.Finished; // Suficientemente cerca para intentar comer
        }
    }

    public void GenerarPatitos()
    {
        energia -= 20;//resta energfia y aumenta hambre
        hambre += 10;
        Instantiate(patito,transform.position,transform.rotation); //se generarn patitos en la posicion actual
        
       
    }
}

