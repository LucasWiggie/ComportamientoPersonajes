using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cocodrilo : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public bool puedeVer;

    // BTs de cada acci�n
    public GameObject BT_Hambre;
    public GameObject BT_Energia;
    public GameObject BT_Miedo;

    //Rango 0-100 las 3 
    public float hambre; 
    public float energia;
    public float miedo;

    // Utilidades
    public float _hambre;
    public float _energia;
    public float _miedo;
    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    //NavMeshAgent
    private NavMeshAgent crocNav;
    public Transform animalTarget;

    //Collider el objeto con el que se ha chocado
    private Collider collidedObject;

    private bool isDefaultMov = true;
    private bool dirtyUS = false;

    //Para indicar si esta aSalvo
    public Pato patoScript; // Aseg�rate de asignar esto desde el Inspector
    public Castor castorScript; 

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

        hambre = 90;
        energia = 100;
        miedo = 0;

        _hambre = hambre;
        _energia = energia;
        _miedo = miedo;

        UtilitySystem();

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
            crocNav.SetDestination(randomPoint); // Establecer el punto como destino

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
            ComprobarVision();
        }
    }

    public void ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;

            if (target.CompareTag("Pato"))
            {
                // Acceder a la variable aSalvo de Pato
                bool estaASalvo = patoScript.aSalvo;

                Vector3 directionToTarget = (target.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

                if (dotProduct > angleThreshold)
                {
                    float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                    {

                        // Verificar si el tag es "Castor" o "Pato"
                        if (estaASalvo == false)
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
                else
                {
                    puedeVer = false;
                }
            }
            else if (target.CompareTag("Castor"))
            {
                // Acceder a la variable aSalvo de Castor
                bool estaASalvo = castorScript.aSalvo;

                Vector3 directionToTarget = (target.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

                if (dotProduct > angleThreshold)
                {
                    float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                    {
                        if (estaASalvo == false)
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
                else
                {
                    puedeVer = false;
                }
            }
        }
        else if (puedeVer)
        {
            puedeVer = false;
        }
    }


    public void UtilitySystem()
    {
        _hambre = this.getHambre();
        _energia = this.getEnergia();
        _miedo = this.getMiedo();

        if (_energia < 50)
        {
            EnergiaAction();
        } 
        else if (_hambre > 70 && _hambre > _miedo && _energia > 50)
        {
            HambreAction();
        }
        else if (_miedo > 80 && _miedo > _hambre && _energia > 50)
        {
            MiedoAction();
        }
    }

    public void HambreAction()
    {
        // BT de cuando el Cocodrilo tiene hambre
        BT_Hambre.SetActive(true);
    }

    public void EnergiaAction()
    {
        // BT de cuando el Cocodrilo tiene poca energ�a
    }

    public void MiedoAction()
    {
        // BT de cuando el Cocodrilo tiene miedo
    }

    //Acci�n perseguir
    public enum ChaseState
    {
        Finished,
        Failed
    }

    public ChaseState Chase()
    {
        
        float minDist = crocNav.stoppingDistance;
        if (animalTarget != null)
        {
            float dist = Vector3.Distance(animalTarget.position, transform.position);
            crocNav.speed = crocNav.speed + 1;
            while (dist > minDist)
            {
                if(animalTarget == null) //si el animal se ha muerto por el camino
                {
                    break;//salimos del bucle
                }
                
                crocNav.SetDestination(animalTarget.position); //se pone como punto de destino la posicion del animal
                
            }
            return ChaseState.Finished;// se ha llegado al punto indicado aunque el animal ya no este (muerto o escondido)
           
       
            
        }
        else
        {
            return ChaseState.Failed; //no haya animal al que perseguir
        }
        
    }
}
