using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


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
    public LayerMask huirCocodrilo;

    public bool puedeVer;
    public bool puedeVerPresa;

    public bool cogePalo = false;
    public bool dejaPalo = false;
    public bool llegaDestino = false;
    private static List<Transform> palosRecogidos = new List<Transform>();
        

    public enum ChaseState
    {
        Finished,
        Failed, 
        Enproceso
    }

    // BTs de cada acci�n
    public GameObject btHambre;
    public GameObject btEnergiaMiedo;
    public GameObject btPalosPresa; // *Acci�n por defecto

    //Bool bts
    private bool boolHambre = false;
    private bool boolEnergia = false;
    private bool boolMiedo = false;

    // Utilidades
    public float uHambre;
    public float uEnergia;
    public float uMiedo;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    float hambreRate = 2f;
    float energiaRate = 2f;

    private bool isDefaultMov = true;

    public bool aSalvo = false;
    private bool descansando = false;

    //NavMeshAgent
    public NavMeshAgent castNav;

    //Objetivo
    public Transform paloTarget;
    public Transform presaTarget;

    //Peligros
    private List<Transform> cocodrilosCercanos = new List<Transform>();
    public float distanciaMaxima = 2f;

    public Bark bark;

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

        hambre = Random.Range(30, 50);
        energia = Random.Range(80, 100);
        miedo = 0;

        uHambre = hambre;
        uEnergia = energia;
        uMiedo = miedo;

        bark = GetComponentInChildren<Bark>();

    }

    private void Update()
    {
        UpdateVariables();
        DetectarObjetivos();
        ActualizarMiedo();

        if (paloTarget != null)
        {
            float distanciaPalo = Vector3.Distance(transform.position, paloTarget.position);

            if (distanciaPalo <= 1f && !cogePalo)
            {
                CogerPalo();
            }
        }

        if (presaTarget != null)
        {
            float distanciaPresa = Vector3.Distance(transform.position, presaTarget.position);

            if (distanciaPresa <= 1f && cogePalo)
            {
                SoltarPalo();
            }
        }
        if(hambre>80 && cogePalo){
            comerPalo();
        }
    }
    private void FixedUpdate()
    {
        UtilitySystem();
        if (isDefaultMov)
        {
            btPalosPresa.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolHambre)
        {
            btHambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedo || boolEnergia)
        {
            btEnergiaMiedo.GetComponent<MonoBehaviourTree>().Tick();
        }
    }

    public void UtilitySystem()
    {
        uHambre = this.getHambre();
        uEnergia = this.getEnergia();
        uMiedo = this.getMiedo();


        if (uHambre >= 100 || uEnergia <= 0)
        {
            Debug.Log(this.gameObject + " ha muerto");
            castNav = null;
            Destroy(this.gameObject);
        }
        else if (uMiedo > 90)
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            boolMiedo = true;
            aSalvo = false;
            btHambre.SetActive(false);
            btPalosPresa.SetActive(false);
            btEnergiaMiedo.SetActive(true);
        }
        else if (uEnergia < 30  || descansando)
        {
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = false;
            boolEnergia = true;
            aSalvo = false;
            btHambre.SetActive(false);
            btPalosPresa.SetActive(false);
            btEnergiaMiedo.SetActive(true);

            bark.gameObject.SetActive(true);
            bark.ChangeImage(3);

        }
        else if (uHambre > 80)
        {
            boolEnergia = false;
            boolMiedo = false;
            isDefaultMov = false;
            boolHambre = true;
            aSalvo = false;
            btPalosPresa.SetActive(false);
            btEnergiaMiedo.SetActive(false);
            btHambre.SetActive(true);

            bark.gameObject.SetActive(true);
            bark.ChangeImage(0);

        }
        else if (uMiedo > 70)
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            boolMiedo = true;
            aSalvo = false;
            btHambre.SetActive(false);
            btPalosPresa.SetActive(false);
            btEnergiaMiedo.SetActive(true);

            bark.gameObject.SetActive(true);
            bark.ChangeImage(1);

        }
        else
        {
            boolEnergia = false;
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = true;
            aSalvo = false;
            btEnergiaMiedo.SetActive(false);
            btHambre.SetActive(false);
            btPalosPresa.SetActive(true);

            bark.gameObject.SetActive(true);
            bark.ChangeImage(2);
        }
    }

        private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);

    }

    void SoltarPalo()
    {
        if (paloTarget != null)
        {
            // Verificar que el palo esté siendo llevado por el castor
            if (paloTarget.parent == transform)
            {
                // Soltar el palo
                paloTarget.parent = null;
                dejaPalo = true;
                cogePalo = false;

                // Destruir el palo
                Destroy(paloTarget.gameObject);
                paloTarget = null;

                Debug.Log("Palo dejado en la presa");
            }
        }
    }

    void CogerPalo()
    {
        if (paloTarget != null)
        {
            Debug.Log("Cogió el palo");
            paloTarget.parent = transform;
            dejaPalo = false;
            cogePalo = true;
        }
    }

    public ChaseState ComprobarVision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);
        Debug.Log("cuenta " + rangeChecks.Length);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // Verificar que el objetivo esté dentro del ángulo de visión
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));
                if (dotProduct > angleThreshold)
                {
                    float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                    // Verificar que no haya obstrucciones entre el castor y el palo
                    if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                    {
                        // Verificar si el palo ha sido recogido por otro castor
                        if (!Castor.PaloRecogido(target))
                        {
                            // Si este objetivo está más cerca que los anteriores, actualizar el más cercano
                            if (distanciaToTarget < closestDistance)
                            {
                                closestDistance = distanciaToTarget;
                                closestTarget = target;
                            }
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar el palo más cercano como el objetivo
                paloTarget = closestTarget;
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

    public ChaseState HayPresa()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioPresa, targetMaskPresa);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // Verificar que el objetivo esté dentro del ángulo de visión
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));
                if (dotProduct > angleThreshold)
                {
                    float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                    // Verificar que no haya obstrucciones entre el castor y la presa
                    if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                    {
                        // Si este objetivo está más cerca que los anteriores, actualizar el más cercano
                        if (distanciaToTarget < closestDistance)
                        {
                            closestDistance = distanciaToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar la presa más cercana como el objetivo
                presaTarget = closestTarget;
                puedeVerPresa = true;
                return ChaseState.Finished;
            }
            else
            {
                puedeVerPresa = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVerPresa)
        {
            puedeVerPresa = false;
            return ChaseState.Failed;
        }

        return ChaseState.Failed;
    }

    public ChaseState irPresa()
    {
        try
        {
            if (presaTarget != null)
            {
                castNav?.SetDestination(presaTarget.position);
                float distanciaX = Mathf.Abs(transform.position.x - presaTarget.position.x);
                float distanciaZ = Mathf.Abs(transform.position.z - presaTarget.position.z);

                if (distanciaX <= 2 && distanciaZ <= 2)
                {
                    Debug.Log("en presa");
                    aSalvo = true;
                    castNav.speed = 3.5f;
                    // Incrementa la energía del castor mientras está en la presa
                    energia += 0.4f;
                    //hambreRate = 0.005f;
                    descansando = true;
                    if (energia > 90)//una cantidad necesaria de energia que reponer para poder salir del nenufar, evitando cambios de comportamiento por cte por el cambio del valor de energia en la franja de cansancio
                    {
                        descansando = false;
                    }

                    return ChaseState.Enproceso;
                }
                else
                {
                    if (miedo > 70)
                    {
                        energia -= 0.02f;
                        castNav.speed = 3.75f;
                    }
                }
                return ChaseState.Enproceso;
            }
            else
            {
                Debug.Log("presa null");
                return ChaseState.Failed;
            }
        }
        catch { return ChaseState.Failed; }
    }

    public ChaseState irPalo()
    {
        try
        {
            Debug.Log("Ir por el palo");
            castNav.stoppingDistance = 0;

            if (paloTarget != null)
            {
                castNav?.SetDestination(paloTarget.position);

                if (cogePalo)
                {
                    return ChaseState.Finished;
                }
                return ChaseState.Enproceso;
            }
            else
            {
                Debug.Log("Fallo al buscar el palo");
                ComprobarVision();
                return ChaseState.Failed;
            }
        }
        catch
        {
            return ChaseState.Failed;
        }
    }

    public ChaseState llevarAPresa()
    {
        try
        {
            Debug.Log("Lleva palo a la presa");
            HayPresa();

            castNav.stoppingDistance = 0;

            if (presaTarget != null)
            {
                castNav?.SetDestination(presaTarget.position);

                if (dejaPalo)
                {
                    Debug.Log("Llegó a la presa");
                    return ChaseState.Finished;
                }
                return ChaseState.Enproceso;
            }
            else
            {
                Debug.Log("Fallo al buscar la presa");
                return ChaseState.Failed;
            }
        }
        catch
        {
            return ChaseState.Failed;
        }
    }

    public ChaseState comerPalo()
    {
        if (cogePalo)
        {
            SoltarPalo();
            hambre -= 30;
            return ChaseState.Finished;

        }
        else { Debug.Log("no hay palo cogio"); return ChaseState.Failed; }


    }

    void DetectarObjetivos()
    {
        cocodrilosCercanos.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, distanciaMaxima, huirCocodrilo);
        foreach (Collider collider in colliders)
        {
            cocodrilosCercanos.Add(collider.transform);
        }
    }

    void ActualizarMiedo()
    {
        miedo = 0f;

        foreach (Transform objetivo in cocodrilosCercanos)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.position);
            float miedoIncremental = Mathf.Clamp01(1 - distancia / distanciaMaxima) * 100; // Calcular el miedo incremental normalizado

            miedo = Mathf.Max(miedo, miedoIncremental); // Mantener el mayor valor de miedo
        }
    }


    public static bool PaloRecogido(Transform palo)
    {
        Castor[] castores = FindObjectsOfType<Castor>(); // Obtener todos los castores en la escena

        foreach (Castor castor in castores)
        {
            if (castor.paloTarget == palo && castor.cogePalo) // Verificar si un castor tiene como objetivo el palo y lo ha recogido
            {
                return true; // El palo ha sido recogido por algún castor
            }
        }

        return false; // El palo no ha sido recogido por ningún castor
    }

    public static void AgregarPaloRecogido(Transform palo)
    {
        if (!palosRecogidos.Contains(palo))
        {
            palosRecogidos.Add(palo);
        }
    }
}
