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

    public float hambre = 60; //Rango 0-100 las 3
    public float energia = 100;
    public float miedo = 0; 
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
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el próximo movimiento
        }
    }

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 3f;

    // Función para encontrar un punto aleatorio en el NavMesh dentro de un radio dado
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

    public bool ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
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

                }
                else
                {
                    puedeVer = false;
                }
            }
            else
            {
                puedeVer = false;
            }
        }
        else if (puedeVer)
        {
            puedeVer = false;
        }
        return puedeVer;
    }

    public bool HayNenufares()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioNenufar, targetMaskNenufar);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
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
                    puedeVerNenufar = true;

                }
                else
                {
                    puedeVerNenufar = false;
                }
            }
            else
            {
                puedeVerNenufar = false;
            }
        }
        else if (puedeVerNenufar)
        {
            puedeVerNenufar = false;
        }
        return puedeVerNenufar;
    }

    //Acción ir a nenufares
    public enum ChaseState
    {
        Finished,
        Failed
    }
    public ChaseState IrNenufares()
    {
        float stopDistance = patoNav.stoppingDistance;
        patoNav.stoppingDistance = 0;
        float minDist = patoNav.stoppingDistance;
        if (nenufarTarget != null)
        {
            float dist = Vector3.Distance(nenufarTarget.position, transform.position); //calcular distancia

            if (dist > minDist) //si no está en el nenufar
            {

                patoNav.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar

            }

            //esperar a que llegue al nenufar
            StartCoroutine(EsperarLlegada());
            //aumentarle la energía una vez llegue al nenufar
            energia = 100;
            patoNav.isStopped= true;
            //esperar a que el pato descanse
            StartCoroutine(ReaunadarMovimiento()); //corutina para reanudar el movimiento despues de x tiempo
            patoNav.stoppingDistance = stopDistance;
            return ChaseState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            patoNav.stoppingDistance = stopDistance;
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
        yield return new WaitForSeconds(10);
        patoNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }

    public ChaseState HuirNenufares()
    {
        float stopDistance = patoNav.stoppingDistance;
        patoNav.stoppingDistance = 0;
        float minDist = patoNav.stoppingDistance;
        if (nenufarTarget != null)
        {
            float dist = Vector3.Distance(nenufarTarget.position, transform.position); //calcular distancia

            if (dist > minDist) //si no está en el nenufar
            {

                patoNav.speed = patoNav.speed + 1;
                patoNav.SetDestination(salTarget.position); //se pone como punto de destino la posicion del nenufar

                while (transform.position != salTarget.position)
                {
                    energia -= 10;
                    energia = Mathf.Clamp(energia, 0f, 100f);
                    if(energia == 0)
                    {
                        break;
                    }
                }

            }
            if (transform.position != salTarget.position)
            {
                patoNav.speed--; //no tiene energía para correr
                //esperar a que llegue al nenufar
                StartCoroutine(EsperarLlegada());
            }
            //aumentarle la energía una vez llegue al nenufar
            miedo = 0;
            patoNav.isStopped = true;
            //esperar a que el pato descanse
            StartCoroutine(ReaunadarMovimiento()); //corutina para reanudar el movimiento despues de x tiempo
            patoNav.stoppingDistance = stopDistance;
            return ChaseState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            patoNav.stoppingDistance = stopDistance;
            return ChaseState.Failed;
        }
    }

    //Acción Comer Salamandra
    public void ComerSal()
    {
        
        GameObject.Destroy(salTarget.GetComponentInParent<GameObject>());//destruimos el gameobject de la salamandra que se ha comido
        hambre = 0;//ya ha comido
    }

    //Acción Perseguir Salamandra
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

