using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Huevo : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;
    private float distanciaMinima = 3.0f;

    public LayerMask targetMask;
    public LayerMask obstructionMask;
    private Transform crocTarget;
    public Salamandra madreSalamandra;
    public Transform transformMadreSalamandra;

    public bool puedeVer;
    public bool aSalvo;

    // Start is called before the first frame update
    void Start()
    {
        aSalvo = false;
    }

    private void FixedUpdate()
    {
        if (HayCroc())
        {
            // Avisar a la salamandra solo si NO está protegiendo ya a otro huevo
            if (!madreSalamandra.boolProtegerHuevos)
            {
                AvisarSalamandra();
            }
        }
        else
        {
            madreSalamandra.boolProtegerHuevos = false;
        }

        aSalvo = Vector3.Distance(transform.position, transformMadreSalamandra.position) < distanciaMinima;
        //if (Vector3.Distance(transform.position, transformMadreSalamandra.position) < distanciaMinima)
        //{
        //    aSalvo = true;
        //}
        //else
        //{
        //    aSalvo = false;
        //}
    }

    public bool HayCroc()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

        //salamandras que no estan resguardadas
        List<Collider> crocsAcechando = new List<Collider>();

        foreach (Collider col in rangeChecks)
        {
            // Obtener el GameObject padre del colisionador
            GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

            // Verificar si el objetivo es una salamandra y si no est� a salvo
            Cocodrilo cocodrilo = targetParent.GetComponent<Cocodrilo>();

            if (cocodrilo != null && cocodrilo.boolEnergia) 
            {
                crocsAcechando.Add(col);
            }
        }

        if (crocsAcechando.Count > 0)
        {
            // Utilizar el primer objetivo no a salvo encontrado
            Transform target = crocsAcechando[0].transform;
            crocTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Utilizar el producto punto para verificar el �ngulo
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            // Establecer un umbral para el �ngulo (ajustar seg�n sea necesario)
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));
            //if (dotProduct > angleThreshold)
            //{
                float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                {
                    return puedeVer = true;
                }
                else
                {
                    return puedeVer = false;
                }
            //}
            //else
            //{
                return puedeVer = false;
            //}
        }
        else if (puedeVer)
        {
            return puedeVer = false;
        }
        return false;
    }

    public void AvisarSalamandra()
    {
        madreSalamandra.boolProtegerHuevos = true;
        madreSalamandra.huevoAProteger = transform;
        madreSalamandra.isDefaultMov = false;
    }
}

