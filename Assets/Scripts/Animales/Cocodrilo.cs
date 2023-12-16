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

    // BTs de cada acción
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
        // BT de cuando el Cocodrilo tiene poca energía
    }

    public void MiedoAction()
    {
        // BT de cuando el Cocodrilo tiene miedo
    }

    //Acción perseguir
    public void Chase()
    {
        crocNav.speed = crocNav.speed + 1;
        crocNav.SetDestination(animalTarget.position);
    }
}
