using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Cocodrilo : MonoBehaviour
{
    public float radio;
    public float radioHuevos;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask targetMaskHuevos;
    public LayerMask targetMaskArena;
    public LayerMask obstructionMask;
    [SerializeField] public LayerMask huirPatito;

    public bool puedeVer;
    public bool puedeVerArena;
    public bool puedeVerHuevos;


    // BTs de cada acci�n
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedo;

    //Bool bts
    private bool boolHambre = false;
    public bool boolEnergia = false;
    private bool boolMiedo = false;
    public bool aSalvo = false;

    //Rango 0-100 las 3 
    public float hambre;
    public float energia;
    public float miedo;

    // Utilidades
    private float uHambre;
    private float uEnergia;
    private float uMiedo;

    float hambreRate = 2f;
    float energiaRate = 2f;

    //NavMeshAgent
    public NavMeshAgent crocNav;

    //Objetivos
    private Transform animalTarget; //animal que va a ser objetivo
    private Transform eggsTarget;//huevos objetivo
    private Transform sandTarget;//arena

    //Collider el objeto con el que se ha chocado
    private Collider collidedObject;

    public bool isDefaultMov = false;

    //Para indicar si esta aSalvo
    public Pato patoScript; // Aseg�rate de asignar esto desde el Inspector
    public Castor castorScript;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 7.0f;

    //Lista con los patitos cercanos al cocodrilo
    public List<Transform> patitosCercanos = new List<Transform>();
    private List<Collider> animalesNoASalvo = new List<Collider>();
    private float distanciaMax = 30f;

    private List<Collider> huevosIndefensos = new List<Collider>();

    public enum ChaseState
    {
        Finished,
        Failed,
        Enproceso
    }

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
        playerRef = this.gameObject;
        crocNav = GetComponent<NavMeshAgent>();

        hambre = 80;
        energia = 30;
        miedo = 0;

        uHambre = hambre;
        uEnergia = energia;
        uMiedo = miedo;

    }
    
    private void Update()
    {
        UpdateVariables();
        DetectarObjetivos();
        ActualizarMiedo();
    }

    private void FixedUpdate()
    {
        UtilitySystem();
        if (isDefaultMov)
        {
            NuevoDestinoAleatorio();
        }
        else if (boolHambre) {
            
            btHambre.GetComponent<MonoBehaviourTree>().Tick();

        }
        else if (boolEnergia)
        {
            
            btEnergia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedo)
        {
            
           btMiedo.GetComponent<MonoBehaviourTree>().Tick();
        }
    }

    # region "Movimiento"
    private void NuevoDestinoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            crocNav.SetDestination(randomPoint); // Establecer el punto como destino
            //Debug.Log("croc se mueve");
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el pr�ximo movimiento
        }
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
    #endregion 
    
    #region "Utility system"
    public void UtilitySystem()
    {
        uHambre = this.getHambre();
        uEnergia = this.getEnergia();
        uMiedo = this.getMiedo();
        animalesNoASalvo = ObtenerAnimalesNoASalvo();

        huevosIndefensos = ObtenerHuevosIndefensos();

        if (uHambre >= 100 || uEnergia <= 0)
        {
            Debug.Log(this.gameObject + " ha muerto");
            Destroy(this.gameObject);
        }
        else if (uMiedo > 90)
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            boolMiedo = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(true);
        }
        else if (uEnergia < 20 && huevosIndefensos.Count != 0)
        {
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov= false;
            boolEnergia = true;
            btHambre.SetActive(false);
            btMiedo.SetActive(false);
            btEnergia.SetActive(true);
        }
        else if (uHambre > 70 &&  animalesNoASalvo.Count!=0)
        {
            boolEnergia = false;
            boolMiedo = false;
            isDefaultMov = false;
            boolHambre = true;
            btEnergia.SetActive(false);
            btMiedo.SetActive(false);
            btHambre.SetActive(true);
        }
        else if (uMiedo > 70)
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            boolMiedo = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(true);
        }
        else
        {
            boolEnergia = false;
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(false);
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
        Castor castor = targetParent.GetComponent<Castor>();
        Pato pato = targetParent.GetComponent<Pato>();

        if ((castor != null && !castor.aSalvo) || (pato != null && !pato.aSalvo))
        {
            animalesNoASalvo.Add(col);
        }
    }

    return animalesNoASalvo;
    }
    #endregion

    #region "Acciones"
    public ChaseState HayCaza()
    {
        Debug.Log("ENTRO EN HAY CAZA");

        // Obtener todos los colisionadores en el rango especificado
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        // Lista para almacenar colisionadores de animales no a salvo
        List<Collider> animalesNoASalvo = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es un castor o un pato y si no está a salvo
            Castor castor = targetParent.GetComponent<Castor>();
            //Pato pato = targetParent.GetComponent<Pato>();

            if ((castor != null && !castor.aSalvo) ) //|| (pato != null && !pato.aSalvo)) HAY QUE METER ESTO QUE NO SE OLVIDEEEEEEEEEEEEEEEEEE
            {
                animalesNoASalvo.Add(col);
            }
        }

        // Verificar si hay objetivos no a salvo
        if (animalesNoASalvo.Count > 0)
        {
            // Utilizar el primer objetivo no a salvo encontrado
            Transform target = animalesNoASalvo[0].transform;
            animalTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Utilizar el producto punto para verificar el ángulo
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            // Establecer un umbral para el ángulo (ajustar según sea necesario)
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

    public ChaseState HayHuevos()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioHuevos, targetMaskHuevos);

        //Huevos que no estan resguardadas
        List<Collider> huevosIndefensos = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es una salamandra y si no est� a salvo
            Huevo huevo = targetParent.GetComponent<Huevo>();

            if ((huevo != null && !huevo.aSalvo))
            {
                huevosIndefensos.Add(col);
            }
        }

        if (huevosIndefensos.Count > 0)
        {
            Transform target = huevosIndefensos[0].transform;
            eggsTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

            if (dotProduct > angleThreshold)
            {
                float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                {
                    puedeVerHuevos = true;
                    return ChaseState.Finished;
                }
                else
                {
                    puedeVerHuevos = false;
                    return ChaseState.Failed;
                }
            }
            else
            {
                puedeVerHuevos = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVerHuevos)
        {
            puedeVerHuevos = false;
            return ChaseState.Failed;
        }

        Debug.Log("devuelve fail? ");
        return ChaseState.Failed;
    }

    public ChaseState HayArena()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMaskArena);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

                if (dotProduct > angleThreshold)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        if (distanceToTarget < closestDistance)
                        {
                            closestDistance = distanceToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar la presa m�s cercana como el objetivo
                sandTarget = closestTarget;
                puedeVer = true;
                return ChaseState.Finished;
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

    //Acci�n perseguir animales
    public ChaseState Perseguir()
    {
        if (animalTarget == null)
        {
            Debug.Log("Chase failed: no target.");
            return ChaseState.Failed; // No hay objetivo
        }

        GameObject targetParent = animalTarget.gameObject.transform.parent.gameObject;

        if(targetParent.tag=="Castor"){
            var castor = targetParent.GetComponent<Castor>();
            if (castor.aSalvo) 
            {
                return ChaseState.Finished;
            }
        }
        else if(targetParent.tag=="Pato"){
            var pato = targetParent.GetComponent<Pato>();
            if (pato.aSalvo)
            {
                return ChaseState.Finished;
            }
        }

        float minDist = crocNav.stoppingDistance;
        float dist = Vector3.Distance(animalTarget.position, transform.position);

        Debug.Log($"Chasing target. Current distance: {dist}, Minimum distance: {minDist}");

        if (dist <= 2.0)
        {
            Debug.Log("Chase finished: target reached.");
            return ChaseState.Finished; // Se ha llegado al objetivo
        }
    
        if (crocNav.pathPending || dist > minDist)
        {
            crocNav.SetDestination(animalTarget.position); // Actualiza el destino
            return ChaseState.Enproceso; // La persecución está en curso
        }

        Debug.Log("Chase finished: close enough to target.");
        return ChaseState.Finished; // Se ha llegado suficientemente cerca
    }

    //Acci�n comer tanto huevos como animales
    public void Eat(bool animal, bool eggs)
    {
        if (animal && animalTarget != null) // Si vamos a comer un animal y hay un objetivo animal
        {
            // Obtener el padre del GameObject que contiene los componentes asociados al objetivo animal
            GameObject targetParent = animalTarget.gameObject.transform.parent.gameObject;
            var castor = targetParent.GetComponent<Castor>();
            var pato = targetParent.GetComponent<Pato>();
            if (castor != null)
            {
                if (!castor.aSalvo)
                {
                    // Destruir el GameObject del animal y su padre
                    GameObject.Destroy(targetParent);
                    // Restablecer la cantidad de hambre a cero
                    hambre = 0;
                }

            }
            if (pato != null)
            {
                if (!pato.aSalvo)
                {
                    // Destruir el GameObject del animal y su padre
                    GameObject.Destroy(targetParent);
                    // Restablecer la cantidad de hambre a cero
                    hambre = 0;
                }

            }

        }
        else if (eggs && eggsTarget != null) // Si vamos a comer huevos y hay un objetivo de huevos
        {
            // Obtener el padre del GameObject que contiene los componentes asociados al objetivo de huevos
            GameObject targetParent = eggsTarget.gameObject;

            StartCoroutine(ReanudarMovimiento(2.0f));

            // Destruir el GameObject de los huevos y su padre
            eggsTarget.gameObject.GetComponent<Huevo>().madreSalamandra.boolProtegerHuevos = false;
            GameObject.Destroy(targetParent);

            // Aumentar la energía
            energia += 20;
            energia = Mathf.Clamp(energia, 0f, 100f);

        }

    }

    //Acci�n huir a arena
    public ChaseState IrArena()
    {
        if (sandTarget != null)
        {
            crocNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            crocNav.speed = crocNav.speed + 5f;
            energiaRate = 0.1f;
            energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                Debug.Log("COCODRILO EN ARENA");
                miedo = 0; //reducimos el miedo
                energiaRate = 0.05f;
                crocNav.isStopped = true;//paramos el movimiento
                StartCoroutine(ReanudarMovimiento(5.0f));
                crocNav.speed = crocNav.speed - 5f;
                return ChaseState.Finished;
            }
            crocNav.speed = crocNav.speed -5f;
            return ChaseState.Enproceso;// se ha llegado al punto indicado aunque el animal ya no este (muerto o escondido)

        }
        else
        {
            crocNav.stoppingDistance = 0;
            crocNav.speed--;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }
    
    public ChaseState IrAHuevos()
    {
        if (eggsTarget != null && !eggsTarget.GetComponent<Huevo>().aSalvo)
        {
            crocNav.SetDestination(eggsTarget.position);

            if (!eggsTarget.GetComponentInParent<Huevo>().aSalvo)
            {
                if ((transform.position.x - eggsTarget.position.x <= 0.5) && (transform.position.z - eggsTarget.position.z <= 0.5))
                {
                    crocNav.isStopped = true;
                    return ChaseState.Finished;
                }
            } 
            else
            {
                return ChaseState.Finished;
            }

            return ChaseState.Enproceso;
        }
        else
        {
            crocNav.stoppingDistance = 2;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }

    public IEnumerator EsperarLlegada()
    {
        yield return new WaitUntil(() => crocNav.remainingDistance <= crocNav.stoppingDistance); //esperar a que el pato llegue a su destino
    }

    public IEnumerator ReanudarMovimiento(float tiempo)
    {
        aSalvo = true;
        yield return new WaitForSeconds(tiempo);
        aSalvo = false;
        crocNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }

    
    #endregion

    #region "Otros"
    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collidedObject = collision.gameObject.GetComponent<Collider>();
    }

    private IEnumerator FOVRoutine()
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (true)
        {
            yield return wait;
            HayCaza();
        }
    }
    #endregion
    void DetectarObjetivos()
    {
        patitosCercanos.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, distanciaMax, huirPatito);
        foreach (Collider collider in colliders)
        {
            patitosCercanos.Add(collider.transform);
        }
    }

    void ActualizarMiedo()
    {
        miedo = 0f;

        foreach (Transform objetivo in patitosCercanos)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.position);
            float miedoIncremental = Mathf.Clamp01(1 - distancia / distanciaMax) * 100; // Calcular el miedo incremental normalizado

            miedo = Mathf.Max(miedo, miedoIncremental); // Mantener el mayor valor de miedo
        }
    }

    public List<Collider> ObtenerHuevosIndefensos()
    {
        // Obtener todos los colisionadores en el rango especificado
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMaskHuevos);

        // Lista para almacenar colisionadores de animales no a salvo
        List<Collider> huevosIndefensos = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            Huevo huevo = targetParent.GetComponent<Huevo>();

            if ((huevo != null && !huevo.aSalvo))
            {
                huevosIndefensos.Add(col);
            }
        }

        return huevosIndefensos;
    }
}
