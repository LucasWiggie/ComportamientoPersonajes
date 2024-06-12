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
    public GameObject BT_Hambre;
    public GameObject BT_EnergiaMiedo;
    public GameObject BT_PalosPresa; // *Acci�n por defecto

    //Bool bts
    private bool bool_Hambre = false;
    private bool bool_Energia = false;
    private bool bool_Miedo = false;


    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;

    public bool cogePalo = false;
    public bool dejaPalo = false;
    public bool llegaDestino = false;


    //NavMeshAgent
    public NavMeshAgent castNav;

    //Objetivo
    public Transform paloTarget;
    public Transform presaTarget;

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

        hambre = 30;
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
            //BT_PalosPresa.SetActive(true);
            BT_PalosPresa.GetComponent<MonoBehaviourTree>().Tick();

            //BT_Hambre.SetActive(true);
            //BT_Hambre.GetComponent<MonoBehaviourTree>().Tick();

            //BT_EnergiaMiedo.SetActive(true);
            //BT_EnergiaMiedo.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (bool_Hambre)
        {
            Debug.Log("Tengo Hambre");
            BT_Hambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (bool_Miedo || bool_Energia)
        {
            BT_EnergiaMiedo.GetComponent<MonoBehaviourTree>().Tick();
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

       // if(hambre<= 20) { }
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

                // Incrementa la energía del castor mientras está en la presa
                energia += 0.08f;

                // Permitir que el castor salga de la presa solo si su energía es >= 90
                if (energia >= 90)
                {
                    return ChaseState.Finished;
                }

                return ChaseState.Enproceso;
            }
            else
            {
                if (miedo > 70)
                {
                    energia -= 0.02f;
                    castNav.speed += 0.002f;
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
            paloTarget.parent = null;
            dejaPalo = true;
            cogePalo = false;
            Destroy(paloTarget.gameObject);
            paloTarget = null;
            
    }
    

    void CogerPalo()
    {
        Debug.Log("cogio");
        paloTarget.parent = transform;
        dejaPalo = false;
        cogePalo = true;
    }

    public void UtilitySystem()
    {
        _hambre = this.getHambre();
        _energia = this.getEnergia();
        _miedo = this.getMiedo();

        if (_energia < 60)
        {
            bool_Hambre = false;
            bool_Miedo = false;
            isDefaultMov = false;
            bool_Energia = true;
            aSalvo = false;
            BT_Hambre.SetActive(false);
            BT_PalosPresa.SetActive(false);
            BT_EnergiaMiedo.SetActive(true);
        }
        else if (_miedo > 70)
        {
            bool_Energia = false;
            bool_Hambre = false;
            isDefaultMov = false;
            bool_Miedo = true;
            aSalvo = false;
            BT_Hambre.SetActive(false);
            BT_PalosPresa.SetActive(false);
            BT_EnergiaMiedo.SetActive(true);
        }
        else if (_hambre > 50)
        {
            bool_Energia = false;
            bool_Miedo = false;
            isDefaultMov = false;
            bool_Hambre = true;
            aSalvo = false;
            BT_PalosPresa.SetActive(false);
            BT_EnergiaMiedo.SetActive(false);
            BT_Hambre.SetActive(true);
        }
        else if (_energia < 90)
        {
            bool_Hambre = false;
            bool_Miedo = false;
            isDefaultMov = false;
            bool_Energia = true;
            aSalvo = true; // Mantener aSalvo en true hasta que la energía alcance 90
            BT_Hambre.SetActive(false);
            BT_PalosPresa.SetActive(false);
            BT_EnergiaMiedo.SetActive(true);
        }
        else
        {
            bool_Energia = false;
            bool_Hambre = false;
            bool_Miedo = false;
            isDefaultMov = true;
            aSalvo = false;
            BT_EnergiaMiedo.SetActive(false);
            BT_Hambre.SetActive(false);
            BT_PalosPresa.SetActive(true);
        }
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


    public void EnergiaMiedoAction()
    {
        // BT de cuando el Cocodrilo tiene poca energ�a
    }
}
