using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Castor : MonoBehaviour
{
    public float radio;
    public float radioPresa;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask targetMaskPresa;
    public LayerMask obstructionMask;
    public bool puedeVer;
    public bool puedeVerPresa;


    // BTs de cada acción
    public GameObject BT_Hambre;
    public GameObject BT_EnergiaMiedo;
    public GameObject BT_PalosPresa; // *Acción por defecto

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    //NavMeshAgent
    private NavMeshAgent castNav;

    //Objetivo
    private Transform paloTarget;

    private bool isDefaultMov = true;
    private bool dirtyUS = false;

    // Utilidades
    public float _hambre;
    public float _energia;
    public float _miedo;

    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    public bool aSalvo = false;

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
        castNav = GetComponent<NavMeshAgent>();

        hambre = 50;
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
            BT_PalosPresa.SetActive(true);
        }

        if (dirtyUS)
        {

        }
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

    public enum ChaseState
    {
        Finished,
        Failed
    }

    public ChaseState ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            paloTarget = target;

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

    public bool HayPresa()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioPresa, targetMaskPresa);

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
                    puedeVerPresa = true;

                }
                else
                {
                    puedeVerPresa = false;
                }
            }
            else
            {
                puedeVerPresa = false;
            }
        }
        else if (puedeVerPresa)
        {
            puedeVerPresa = false;
        }
        return puedeVerPresa;
    }


    public ChaseState irPalo()
    {
        float stopDistance = castNav.stoppingDistance;
        castNav.stoppingDistance = 0;
        float minDist = castNav.stoppingDistance;
        if (paloTarget != null)
        {
            float dist = Vector3.Distance(paloTarget.position, transform.position);

            if (dist > minDist)
            {

                castNav.SetDestination(paloTarget.position); //se pone como punto de destino la posicion de los huevos

            }

            return ChaseState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            castNav.stoppingDistance = stopDistance;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }

    public void UtilitySystem()
    {
        _hambre = this.getHambre();
        _energia = this.getEnergia();
        _miedo = this.getMiedo();

        if (_energia < 50 || _miedo > 80)
        {
            EnergiaMiedoAction();
        }
        else if (_hambre > 70 && _hambre > _miedo && _energia > 50)
        {
            HambreAction();
        }
    }

    public void HambreAction()
    {
        // BT de cuando el Cocodrilo tiene hambre
    }

    public void EnergiaMiedoAction()
    {
        // BT de cuando el Cocodrilo tiene poca energía
    }
}
