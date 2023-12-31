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

    public LayerMask targetMask;
    public LayerMask targetMaskNenufar;
    public LayerMask obstructionMask;
    public bool puedeVer;
    public bool puedeVerNenufar;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    //Utilidades
    public float _hambre;
    public float _energia;
    public float _miedo;

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
        UpdateVariables();
        
    }

    private void FixedUpdate()
    {
        if (isDefaultMov)
        {
            movimientoAleatorio();
        }

        if (dirtyUS)
        {

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

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            salTarget= target;  
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
            Transform target = rangeChecks[0].transform;
            nenufarTarget = target;
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
                    puedeVerNenufar = true;
                    return ChaseState.Finished;

                }
                else
                {
                    puedeVerNenufar = false;
                    return ChaseState.Failed;
                }
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
                energia = 100;//vuelve a tener energia
                patoNav.isStopped= true;   
                StartCoroutine(EsperarLlegada());
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
    //Esperar a llegar al destino
    public IEnumerator EsperarLlegada()
    {
        yield return new WaitUntil(()=>patoNav.remainingDistance <= patoNav.stoppingDistance); //esperar a que el pato llegue a su destino
    }
    //Esperar x  tiempo
    public IEnumerator ReaunadarMovimiento()
    {
        yield return new WaitForSeconds(5f);
        patoNav.isStopped = false; //reanudamos el movimiento despues de x segundos
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
                miedo = 0;//reducir el miedo
                patoNav.isStopped = true;//paramos movimiento
                StartCoroutine(ReaunadarMovimiento());
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
    public ChaseState ComerSal()
    {
        if (salTarget != null)
        {
            Destroy(salTarget.gameObject);//destruimos el gameobject de la salamandra que se ha comido
            hambre -= 30;//baja el hambre
            return ChaseState.Finished;
        }
        else
        {
            Debug.Log("no hay salamandra");
            return ChaseState.Failed;
        }
    }

    //Acci�n Perseguir Salamandra
    public ChaseState PerseguirSal()
    {
        float minDist = patoNav.stoppingDistance;
        if (salTarget != null)
        {
            float dist = Vector3.Distance(salTarget.position, transform.position);
            patoNav.speed = patoNav.speed + 1;
            while (dist > minDist)
            {
                if (salTarget == null) //si el animal se ha muerto por el camino
                {
                    break;//salimos del bucle
                }

                patoNav.SetDestination(salTarget.position); //se pone como punto de destino la posicion de la salamandra

            }
            patoNav.speed--;
            return ChaseState.Finished;// se ha llegado al punto indicado aunque la salamandra ya no este (muerto o escondido)



        }
        else
        {
            patoNav.speed--;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }
}

