using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Salamandra : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public bool puedeVer;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;
    public float temorHuevos;

    // BTs
    // BTs de cada accion
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedoPato;
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
        miedo = 0;
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
            btMiedoPato.GetComponent<MonoBehaviourTree>().Tick();
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

        if (_uEnergia < 50) // Si está cansada, se va a la arena
        {
            boolHambre = false;
            boolEnergia = true;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(true);
            btMiedoPato.SetActive(false);
            btProtegerHuevos.SetActive(false);
        }
        else if (_uHambre > 70 && _uHambre > _uMiedo && _uEnergia > 50)
        {
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPato.SetActive(false);
            btProtegerHuevos.SetActive(false);
        }
        else if (_uMiedo > 70 && _uMiedo > _uHambre && _uEnergia > 50)
        {
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPato.SetActive(false);
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
            btMiedoPato.SetActive(false);
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
}

