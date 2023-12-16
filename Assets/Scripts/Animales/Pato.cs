using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Cocodrilo;

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
        StartCoroutine(FOVRoutine());
    }
    private void Update()
    {
        UpdateVariables();
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
    public enum CaminoState
    {
        Finished,
        Failed
    }
    public CaminoState IrNenufares()
    {
        float stopDistance = patoNav.stoppingDistance;
        patoNav.stoppingDistance = 0;
        float minDist = patoNav.stoppingDistance;
        if (nenufarTarget != null)
        {
            float dist = Vector3.Distance(nenufarTarget.position, transform.position);

            if (dist > minDist)
            {

                patoNav.SetDestination(nenufarTarget.position); //se pone como punto de destino la posicion del nenufar

            }

            return CaminoState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            patoNav.stoppingDistance = stopDistance;
            return CaminoState.Failed; 
        }
    }

}

