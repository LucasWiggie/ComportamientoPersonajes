using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Cocodrilo;
using static UnityEngine.GraphicsBuffer;

public class Patitos : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    private NavMeshAgent patitoNav;
    public float lifeTime = 60f;
    public bool puedeVer;

    private Transform crocTarget;


    // Start is called before the first frame update
    void Start()
    {
        patitoNav = GetComponent<NavMeshAgent>();
        Destroy(gameObject,lifeTime); //se destruye despues de x tiempo
    }

    // Update is called once per frame
    void  FixedUpdate()
    {
        if (HayCroc())
        {
            PerseguirCroc();
        }
        else
        {
            movimientoAleatorio();
        }
    }

    private void movimientoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            patitoNav.SetDestination(randomPoint); // Establecer el punto como destino
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

    public bool HayCroc()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        //salamandras que no estan resguardadas
        List<Collider> crocsNoASalvo = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es una salamandra y si no est� a salvo
            Cocodrilo cocodrilo = targetParent.GetComponent<Cocodrilo>();

            if ((cocodrilo != null)) //&& !cocodrilo.aSalvo)) //PONER CUANDO SE PUEDA PONER A SALVO UN COCODRILO
            {
                crocsNoASalvo.Add(col);
            }
            Debug.Log("CROC NULL");
        }

        // Verificar si hay objetivos no a salvo
        if (crocsNoASalvo.Count > 0)
        {
            Debug.Log("COCODRILOS "+ crocsNoASalvo.Count + crocsNoASalvo[0]);
            // Utilizar el primer objetivo no a salvo encontrado
            Transform target = crocsNoASalvo[0].transform;
            crocTarget = target;

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
                    Debug.Log("LO VEO");
                    return puedeVer = true;

                    
                }
                else
                {
                   return puedeVer = false;
                   
                }
            }
            else
            {
                return puedeVer = false;
               
            }
        }
        else if (puedeVer)
        {
           return puedeVer = false;
           
        }
        return false;
    }

    public void PerseguirCroc()
    {
        if (crocTarget == null)
        {
            Debug.LogError("PerseguirCroc failed: crocTarget is null.");
            return;
        }

        float minDist = patitoNav.stoppingDistance;
        float dist = Vector3.Distance(crocTarget.position, transform.position);

        if (dist <= 2)
        {
            Debug.Log("Chase finished: target reached.");
            ComerCroc(); // Se ha llegado al objetivo, llamar a ComerCroc
        }
        else
        {
            if (!patitoNav.pathPending)
            {
                Debug.Log("Chasing croc target.");
                patitoNav.SetDestination(crocTarget.position); // Actualiza el destino
            }
        }
    }

    public void ComerCroc()
    {
        // Obtener el padre del GameObject que contiene los componentes asociados al objetivo animal
        GameObject targetParent = crocTarget.gameObject.transform.parent.gameObject;
        var cocodrilo = targetParent.GetComponent<Cocodrilo>();
        if (cocodrilo != null)
        {
            if (!cocodrilo.aSalvo)
            {
            GameObject.Destroy(targetParent);//destruimos el gameobject de la salamandra que se ha comido
            }

        }

    }
}
