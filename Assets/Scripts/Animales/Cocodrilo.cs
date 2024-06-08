using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public bool puedeVer;
    public bool puedeVerArena;


    // BTs de cada acci�n
    public GameObject BT_Hambre;
    public GameObject BT_Energia;
    public GameObject BT_Miedo;

    //Bool bts
    private bool Bool_Hambre = false;
    private bool Bool_Energia = false;
    private bool Bool_Miedo = false;

    //Rango 0-100 las 3 
    public float hambre;
    public float energia;
    public float miedo;

    // Utilidades
    private float _uHambre;
    private float _uEnergia;
    private float _uMiedo;

    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    //NavMeshAgent
    public NavMeshAgent crocNav;

    //Objetivos
    private Transform animalTarget; //animal que va a ser objetivo
    private Transform eggsTarget;//huevos objetivo
    private Transform sandTarget;//arena

    //Collider el objeto con el que se ha chocado
    private Collider collidedObject;

    public bool isDefaultMov = false;
    private bool dirtyUS = false;

    //Para indicar si esta aSalvo
    public Pato patoScript; // Aseg�rate de asignar esto desde el Inspector
    public Castor castorScript;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 7.0f;


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

        hambre = 68;
        energia = 100;
        miedo = 0;

        _uHambre = hambre;
        _uEnergia = energia;
        _uMiedo = miedo;

        //InvokeRepeating("NuevoDestinoAleatorio", 0f, movementInterval);
        //InvokeRepeating("UtilitySystem", 0f, 2.0f);
        //StartCoroutine(FOVRoutine());
    }
    private void Update()
    {
        UpdateVariables();
    }

    private void FixedUpdate()
    {
        UtilitySystem();
        if (isDefaultMov)
        {
            NuevoDestinoAleatorio();
        }
        else if (Bool_Hambre) {
            
            //BT_Hambre.GetComponent<MonoBehaviourTree>().Tick();

        }
        else if (Bool_Energia)
        {
            
            BT_Energia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (Bool_Miedo)
        {
            
           BT_Miedo.GetComponent<MonoBehaviourTree>().Tick();
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
        _uHambre = this.getHambre();
        _uEnergia = this.getEnergia();
        _uMiedo = this.getMiedo();

        if (_uEnergia < 50)
        {
            Bool_Hambre = false;
            Bool_Miedo = false;
            isDefaultMov= false;
            Bool_Energia = true;
            BT_Hambre.SetActive(false);
            BT_Miedo.SetActive(false);
            BT_Energia.SetActive(true);
        }
        else if (_uHambre > 70 && _uHambre > _uMiedo && _uEnergia > 50)
        {
            Bool_Energia = false;
            Bool_Miedo = false;
            isDefaultMov = false;
            Bool_Hambre = true;
            BT_Energia.SetActive(false);
            BT_Miedo.SetActive(false);
            BT_Hambre.SetActive(true);
        }
        else if (_uMiedo > 70 && _uMiedo > _uHambre && _uEnergia > 50)
        {
            Bool_Energia = false;
            Bool_Hambre = false;
            isDefaultMov = false;
            Bool_Miedo = true;
            BT_Hambre.SetActive(false);
            BT_Energia.SetActive(false);
            BT_Miedo.SetActive(true);
        }
        else
        {
            Bool_Energia = false;
            Bool_Hambre = false;
            Bool_Miedo = false;
            isDefaultMov = true;
            BT_Hambre.SetActive(false);
            BT_Energia.SetActive(false);
            BT_Miedo.SetActive(false);
        }
    }

    public void HambreAction()
    {
        // BT de cuando el Cocodrilo tiene hambre
        Debug.Log("Activo BT Hambre");
        
    }

    public void EnergiaAction()
    {
        
    }
    
    public void MiedoAction()
    {
        
    }
    #endregion

    #region "Acciones"
    public ChaseState HayCaza()
    {
        Debug.Log("ENTRO EN HAY CAZA");
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);
        Debug.Log(rangeChecks.Length);
        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            animalTarget = target;

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
        /*
        //Debug.Log("ENTRA EN COMPROBAR VISION");
        //Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);
        //bool estaASalvo;
        //puedeVer = false;
        //Debug.Log(rangeChecks.Length);
        //if (rangeChecks.Length > 0)
        //{
        //    Transform target = rangeChecks[0].transform;
        //    Debug.Log(target.name);
        //    if (target.CompareTag("Pato"))
        //    {
        //        animalTarget = target; // ponemos el pato como objetivo
        //        // Acceder a la variable aSalvo de Pato
        //        estaASalvo = animalTarget.GetComponentInParent<Pato>().aSalvo;
        //        Debug.Log("estaASalvo: " + estaASalvo);

        //        Vector3 directionToTarget = (target.position - transform.position).normalized;
        //        float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
        //        float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

        //        if (dotProduct > angleThreshold)
        //        {
        //            float distanciaToTarget = Vector3.Distance(transform.position, target.position);

        //            if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
        //            {
        //                // Verificar si el tag es "Castor" o "Pato"
        //                if (estaASalvo == false)
        //                {
        //                    puedeVer = true;
        //                    return ChaseState.Finished;
        //                }
        //                else
        //                {
        //                    puedeVer = false;
        //                    return ChaseState.Failed;
        //                }
        //            }
        //            else
        //            {
        //                puedeVer = false;
        //                return ChaseState.Failed;
        //            }
        //        }
        //        else
        //        {
        //            puedeVer = false;
        //            return ChaseState.Failed;
        //        }
        //    }           
        //}
        //return ChaseState.Failed;
        */
    }

    public ChaseState HayHuevos()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioHuevos, targetMaskHuevos);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

            if (dotProduct > angleThreshold)
            {
                float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                {
                    puedeVerArena = true;
                    return ChaseState.Finished;

                }
                else
                {
                    puedeVerArena = false;
                    return ChaseState.Failed;

                }
            }
            else
            {
                puedeVerArena = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVerArena)
        {
            puedeVerArena = false;
            return ChaseState.Failed;
        }
        return ChaseState.Failed;
    }

    public ChaseState HayArena()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioHuevos, targetMaskArena);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            sandTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
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

    //Acci�n perseguir animales
    public ChaseState Chase()
    {

        float minDist = crocNav.stoppingDistance;
        if (animalTarget != null)
        {
            float dist = Vector3.Distance(animalTarget.position, transform.position);
            crocNav.speed = crocNav.speed + 1;
            while (dist > minDist)
            {
                if (animalTarget == null) //si el animal se ha muerto por el camino
                {
                    break;//salimos del bucle
                }

                crocNav.SetDestination(animalTarget.position); //se pone como punto de destino la posicion del animal

            }
            crocNav.speed--;
            return ChaseState.Finished;// se ha llegado al punto indicado aunque el animal ya no este (muerto o escondido)



        }
        else
        {
            crocNav.speed--;
            return ChaseState.Failed; //no haya animal al que perseguir
        }

    }

    //Acci�n comer tanto huevos como animales
    public void Eat(bool animal, bool eggs)
    {
        GameObject target;
        if (animal) //si vamos a comer un animal
        {
            target = animalTarget.GetComponentInParent<GameObject>();
            GameObject.Destroy(target);//destruimos el gameobject del animal que se ha comido
        }
        else if (eggs) //si comer huevos de salamandra
        {
            target = eggsTarget.GetComponentInParent<GameObject>();
            GameObject.Destroy(target);//destruimos el gameobject de los huevos que se ha comido
            energia += 20; //aumentamos la energ�a
            energia = Mathf.Clamp(energia, 0f, 100f);
        }

        hambre = 0; //ya no hay hambre
    }

    //Acci�n huir a arena
    public ChaseState irArena()
    {
        if (sandTarget != null)
        {
            crocNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            crocNav.speed = crocNav.speed + 5f;
            energia -= 5;
            energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                Debug.Log("COCODRILO EN ARENA");
                miedo = 0; //reducimos el miedo
                crocNav.isStopped = true;//paramos el movimiento
                StartCoroutine(ReanudarMovimiento());
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
    

    public IEnumerator EsperarLlegada()
    {
        yield return new WaitUntil(() => crocNav.remainingDistance <= crocNav.stoppingDistance); //esperar a que el pato llegue a su destino
    }

    public IEnumerator ReanudarMovimiento()
    {
        yield return new WaitForSeconds(5f);
        crocNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }

    public ChaseState IrAHuevos()
    {
        float stopDistance = crocNav.stoppingDistance;
        crocNav.stoppingDistance = 0;
        float minDist = crocNav.stoppingDistance;
        if (eggsTarget != null)
        {
            float dist = Vector3.Distance(eggsTarget.position, transform.position);

            if (dist > minDist)
            {

                crocNav.SetDestination(eggsTarget.position); //se pone como punto de destino la posicion de los huevos

            }

            return ChaseState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            crocNav.stoppingDistance = stopDistance;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
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
}
