using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Salamandra : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public LayerMask targetMaskHuevos;
    public LayerMask targetMaskArena;
    public bool puedeVer;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;
    public float temorHuevos;

    public enum ChaseState
    {
        Finished,
        Failed,
        Enproceso
    }

    // BTs
    // BTs de cada accion
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedoPatos;
    public GameObject btProtegerHuevos;

    //Bool bts
    private bool boolHambre = false;
    private bool boolEnergia = false;
    private bool boolMiedoPato = false;
    private bool boolProtegerHuevos = false;

    //Utilidades
    public float _uHambre;
    public float _uEnergia;
    public float _uMiedo;
    public float _uTemorHuevos;

    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    public bool isDefaultMov = true;
    private bool dirtyUS = false;

    public bool aSalvo = false;

    private NavMeshAgent salamandraNav;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 5f;

    // Objetivos
    private Transform eggsTarget;//huevos objetivo
    private Transform sandTarget;//arena

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
    public float getTemorHuevos()
    {
        return this.temorHuevos;
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
    public void setTemorHuevos(float t)
    {
        this.temorHuevos = t;
    }

    private void Start()
    {
        playerRef = this.gameObject;
        salamandraNav = GetComponent<NavMeshAgent>();
        //InvokeRepeating("NuevoDestinoAleatorio", 0f, movementInterval);

        hambre = 60;
        energia = 100;
        miedo = 51;
        temorHuevos = 0;

        _uHambre = hambre;
        _uEnergia = energia;
        _uMiedo = miedo;
        _uTemorHuevos = temorHuevos;


        StartCoroutine(FOVRoutine());
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
            movimientoAleatorio();
        }
        else if (boolHambre)
        {
            Debug.Log("SAL: Tengo Hambre");
            btHambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolEnergia)
        {
            Debug.Log("SAL: Tengo poca energía");
            btEnergia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedoPato)
        {
            Debug.Log("SAL: Tengo miedo al pato");
            btMiedoPatos.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolProtegerHuevos)
        {
            Debug.Log("SAL: Tengo que proteger los huevos");
            btProtegerHuevos.GetComponent<MonoBehaviourTree>().Tick();
        }

        //if (dirtyUS)
        //{

        //}
    }

    private void movimientoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            salamandraNav.SetDestination(randomPoint); // Establecer el punto como destino
            //Debug.Log("pato se mueve");
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el próximo movimiento
        }
    }

    //private void NuevoDestinoAleatorio()
    //{
    //    if (isDefaultMov)
    //    {
    //        Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
    //        salamandraNav.SetDestination(randomPoint); // Establecer el punto como destino
    //    }    
    //}

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

    public void UtilitySystem()
    {
        _uHambre = this.getHambre();
        _uEnergia = this.getEnergia();
        _uMiedo = this.getMiedo();
        _uTemorHuevos = this.getTemorHuevos();

        if (_uMiedo > 50) 
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = true;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(true);
            btProtegerHuevos.SetActive(false);
        }
        else if (_uTemorHuevos > 60)
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btProtegerHuevos.SetActive(true);
        }
        else if (_uEnergia < 50)
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = true;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(true);
            btMiedoPatos.SetActive(false);
            btProtegerHuevos.SetActive(false);
        }
        else if (_uHambre > 70)
        {
            isDefaultMov = false;
            boolHambre = true;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(true);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btProtegerHuevos.SetActive(false);
        }
        else
        {
            isDefaultMov = true;

            aSalvo = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btProtegerHuevos.SetActive(false);
        }
    }

    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);
    }

    // NO LO HE MIRADO TODAVÍA, ECHADLE UN OJO. Lucas
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

    private void ComprobarVision()
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
    }

    public ChaseState HayArena()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMaskArena);
        Debug.Log("Number of objects found: " + rangeChecks.Length);

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
                // Asignar la presa más cercana como el objetivo
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

    public ChaseState irArena()
    {
        if (sandTarget != null)
        {
            salamandraNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            salamandraNav.speed = salamandraNav.speed + 5f;
            energia -= 5;
            energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                Debug.Log("SALAMANDRA EN ARENA");
                miedo = 0; //reducimos el miedo
                salamandraNav.isStopped = true;//paramos el movimiento
                StartCoroutine(ReanudarMovimiento());
                salamandraNav.speed = salamandraNav.speed - 5f;
                return ChaseState.Finished;
            }
            salamandraNav.speed = salamandraNav.speed - 5f;
            return ChaseState.Enproceso;// se ha llegado al punto indicado aunque el animal ya no este (muerto o escondido)

        }
        else
        {
            salamandraNav.stoppingDistance = 0;
            salamandraNav.speed--;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }

    public IEnumerator ReanudarMovimiento()
    {
        yield return new WaitForSeconds(5f);
        salamandraNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }


}

