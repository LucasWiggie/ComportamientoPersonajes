using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castor : MonoBehaviour
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
    public GameObject BT_EnergiaMiedo;
    public GameObject BT_PalosPresa; // *Acci�n por defecto

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

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

    private void ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
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
        // BT de cuando el Cocodrilo tiene poca energ�a
    }
}
