using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public LayerMask huirCocodrilo;

    public bool puedeVer;
    public bool puedeVerPresa;

    //Lista con los cocodrilos cercanos al castor
    private List<Transform> cocodrilosCercanos = new List<Transform>();
    public float distanciaMaxima = 2f;



    // BTs de cada acci�n
    public GameObject btHambre;
    public GameObject btEnergiaMiedo;
    public GameObject btPalosPresa; // *Acci�n por defecto

    //Bool bts
    private bool boolHambre = false;
    private bool boolEnergia = false;
    private bool boolMiedo = false;


    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    public bool cogePalo = false;
    public bool dejaPalo = false;
    public bool llegaDestino = false;
    private static List<Transform> palosRecogidos = new List<Transform>();


    //NavMeshAgent
    public NavMeshAgent castNav;

    //Objetivo
    public Transform paloTarget;
    public Transform presaTarget;

    private bool isDefaultMov = true;

    // Utilidades
    public float uHambre;
    public float uEnergia;
    public float uMiedo;

    float hambreRate = 2f;
    float energiaRate = 2f;

    public bool aSalvo = false;
    private bool descansando = false;


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

        hambre = 30;
        energia = 70;
        miedo = 0;

        uHambre = hambre;
        uEnergia = energia;
        uMiedo = miedo;

        StartCoroutine(FOVRoutine());
    }

    private void Update()
    {
        UpdateVariables();
        DetectarObjetivos();
        ActualizarMiedo();

        if (paloTarget != null) { 
            if (transform.position.x == paloTarget.position.x && transform.position.z == paloTarget.position.z && !cogePalo)
            {
                CogerPalo();
            }
        }

        if (presaTarget != null)
        {
            if (transform.position.x == presaTarget.position.x && transform.position.z == presaTarget.position.z && !dejaPalo)
            {
                SoltarPalo();
            }
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

    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);

    }

    public void UtilitySystem()
    {
        uHambre = this.getHambre();
        uEnergia = this.getEnergia();
        uMiedo = this.getMiedo();


        if (uHambre >= 100 || uEnergia <= 0)
        {
            Debug.Log(this.gameObject + " ha muerto");
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
        else if (uEnergia < 20  || descansando)
        {
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = false;
            boolEnergia = true;
            aSalvo = false;
            btHambre.SetActive(false);
            btPalosPresa.SetActive(false);
            btEnergiaMiedo.SetActive(true);
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
        }
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
        Failed, 
        Enproceso
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


    public ChaseState irPalo()
    {
        Debug.Log("irpalo");
        castNav.stoppingDistance = 0;

        if (paloTarget != null)
        {
            castNav.SetDestination(paloTarget.position);
            //StartCoroutine(EsperarLlegada());
            if (cogePalo)
            {
                return ChaseState.Finished;
            }
            return ChaseState.Enproceso;
        }
        else
        {
            Debug.Log("fallo");
            ComprobarVision();
            return ChaseState.Failed;
        }
    }

    public ChaseState irPresa()
    {
        if (presaTarget != null)
        {
            castNav.SetDestination(presaTarget.position);
            float distanciaX = Mathf.Abs(transform.position.x - presaTarget.position.x);
            float distanciaZ = Mathf.Abs(transform.position.z - presaTarget.position.z);

            if (distanciaX <= 2 && distanciaZ <= 2)
            {
                Debug.Log("en presa");
                aSalvo = true;
                castNav.speed = 3.5f;
                // Incrementa la energía del castor mientras está en la presa
                energia += 0.4f;
                hambreRate -= 0.005f;
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
    public ChaseState llevarAPresa()
    {
        Debug.Log("lleva palo a presa");
        HayPresa();
        
        castNav.stoppingDistance = 0;

        if (presaTarget != null)
        {
            castNav.SetDestination(presaTarget.position);
            if (dejaPalo)
            {
                Debug.Log("finished llevarapresa");
                return ChaseState.Finished;
            }
            return ChaseState.Enproceso;

        }
        else { Debug.Log("presa null"); return ChaseState.Failed; }
    
    }

    public ChaseState comerPalo()
    {
        if (cogePalo)
        {
            SoltarPalo();
            hambre -= 20;
            return ChaseState.Finished;

        }
        else { Debug.Log("no hay palo cogio"); return ChaseState.Failed; }


    }

    public IEnumerator EsperarLlegada() 
    { 
        yield return new WaitUntil(()=> castNav.remainingDistance <= castNav.stoppingDistance);
    }

    void SoltarPalo()
    {
        if (paloTarget != null)
        {
            // Verificar que el palo tenga un padre antes de soltarlo
            if (paloTarget.parent == transform)
            {
                // Desasignar el padre del palo
                paloTarget.parent = null;
                dejaPalo = true;
                cogePalo = false;
                Destroy(paloTarget.gameObject); // Destruir el palo que estaba llevando el castor
                paloTarget = null;
            }
        }
    }
    

    void CogerPalo()
    {
        Debug.Log("cogio");
        paloTarget.parent = transform;
        dejaPalo = false;
        cogePalo = true;
    }


    /*public void HambreAction()
    {
        // BT de cuando el Cocodrilo tiene hambre
        //moverse a palo
        //_hambre = 0;
    }*/

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


    // Método estático para verificar si un palo ha sido recogido
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

    // Método para agregar un palo recogido a la lista
    public static void AgregarPaloRecogido(Transform palo)
    {
        if (!palosRecogidos.Contains(palo))
        {
            palosRecogidos.Add(palo);
        }
    }

    public void EnergiaMiedoAction()
    {
        // BT de cuando el Cocodrilo tiene poca energ�a
    }
}
