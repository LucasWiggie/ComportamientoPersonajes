using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cocodrilo : MonoBehaviour
{
    public float radio;
    public float radioHuevos;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask targetMaskHuevos;
    public LayerMask targetMaskArena;
    public LayerMask obstructionMask;

    public bool puedeVer;
    public bool puedeVerArena;


    // BTs de cada acci�n
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedo;

    //Bool bts
    private bool boolHambre = false;
    private bool boolEnergia = false;
    private bool boolMiedo = false;

    //Rango 0-100 las 3 
    public float hambre;
    public float energia;
    public float miedo;

    // Utilidades
    private float _uHambre;
    private float _uEnergia;
    private float _uMiedo;

    float hambreRate = 0.2f;
    float energiaRate = 0.05f;

    //NavMeshAgent
    public NavMeshAgent crocNav;

    //Objetivos
    private Transform animalTarget; //animal que va a ser objetivo
    private Transform eggsTarget;//huevos objetivo
    private Transform sandTarget;//arena

    //Collider el objeto con el que se ha chocado
    private Collider collidedObject;

    public bool isDefaultMov = false;
    private bool dirtyUS = false;

    //Para indicar si esta aSalvo
    public Pato patoScript; // Aseg�rate de asignar esto desde el Inspector
    public Castor castorScript;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 7.0f;

    public enum ChaseState
    {
        Finished,
        Failed,
        Enproceso
    }

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

        hambre = 68;
        energia = 100;
        miedo = 0;

        _uHambre = hambre;
        _uEnergia = energia;
        _uMiedo = miedo;

        //InvokeRepeating("NuevoDestinoAleatorio", 0f, movementInterval);
        //InvokeRepeating("UtilitySystem", 0f, 2.0f);
        //StartCoroutine(FOVRoutine());
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
            NuevoDestinoAleatorio();
        }
        else if (boolHambre) {
            
            btHambre.GetComponent<MonoBehaviourTree>().Tick();

        }
        else if (boolEnergia)
        {
            
            btEnergia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedo)
        {
            
           btMiedo.GetComponent<MonoBehaviourTree>().Tick();
        }
    }

    # region "Movimiento"
    private void NuevoDestinoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            crocNav.SetDestination(randomPoint); // Establecer el punto como destino
            //Debug.Log("croc se mueve");
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el pr�ximo movimiento
        }
    }

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
    #endregion 
    
    #region "Utility system"
    public void UtilitySystem()
    {
        _uHambre = this.getHambre();
        _uEnergia = this.getEnergia();
        _uMiedo = this.getMiedo();
        List<Collider> animalesNoASalvo = ObtenerAnimalesNoASalvo();

        if (_uEnergia < 50)
        {
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov= false;
            boolEnergia = true;
            btHambre.SetActive(false);
            btMiedo.SetActive(false);
            btEnergia.SetActive(true);
        }
        else if (_uHambre > 70 && _uHambre > _uMiedo && _uEnergia > 50 && animalesNoASalvo.Count!=0)
        {
            boolEnergia = false;
            boolMiedo = false;
            isDefaultMov = false;
            boolHambre = true;
            btEnergia.SetActive(false);
            btMiedo.SetActive(false);
            btHambre.SetActive(true);
        }
        else if (_uMiedo > 70 && _uMiedo > _uHambre && _uEnergia > 50)
        {
            boolEnergia = false;
            boolHambre = false;
            isDefaultMov = false;
            boolMiedo = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(true);
        }
        else
        {
            boolEnergia = false;
            boolHambre = false;
            boolMiedo = false;
            isDefaultMov = true;
            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedo.SetActive(false);
        }
    }

    public List<Collider> ObtenerAnimalesNoASalvo()
    {
    // Obtener todos los colisionadores en el rango especificado
    Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

    // Lista para almacenar colisionadores de animales no a salvo
    List<Collider> animalesNoASalvo = new List<Collider>();

    foreach (Collider col in rangeChecks)
    {
        // Obtener el GameObject padre del colisionador
        GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

        // Verificar si el objetivo es un castor o un pato y si no está a salvo
        Castor castor = targetParent.GetComponent<Castor>();
        Pato pato = targetParent.GetComponent<Pato>();

        if ((castor != null && !castor.aSalvo) || (pato != null && !pato.aSalvo))
        {
            animalesNoASalvo.Add(col);
        }
    }

    return animalesNoASalvo;
    }

    public void HambreAction()
    {
        // BT de cuando el Cocodrilo tiene hambre
        Debug.Log("Activo BT Hambre");
        
    }

    public void EnergiaAction()
    {
        
    }
    
    public void MiedoAction()
    {
        
    }
    #endregion

    #region "Acciones"
public ChaseState HayCaza()
{
    Debug.Log("ENTRO EN HAY CAZA");

    // Obtener todos los colisionadores en el rango especificado
    Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);

    // Lista para almacenar colisionadores de animales no a salvo
    List<Collider> animalesNoASalvo = new List<Collider>();

    foreach (Collider col in rangeChecks)
    {
        // Obtener el GameObject padre del colisionador
        GameObject targetParent = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

        // Verificar si el objetivo es un castor o un pato y si no está a salvo
        Castor castor = targetParent.GetComponent<Castor>();
        //Pato pato = targetParent.GetComponent<Pato>();

        if ((castor != null && !castor.aSalvo) ) //|| (pato != null && !pato.aSalvo)) HAY QUE METER ESTO QUE NO SE OLVIDEEEEEEEEEEEEEEEEEE
        {
            animalesNoASalvo.Add(col);
        }
    }

    // Verificar si hay objetivos no a salvo
    if (animalesNoASalvo.Count > 0)
    {
        // Utilizar el primer objetivo no a salvo encontrado
        Transform target = animalesNoASalvo[0].transform;
        animalTarget = target;

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

        /*
        //Debug.Log("ENTRA EN COMPROBAR VISION");
        //Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);
        //bool estaASalvo;
        //puedeVer = false;
        //Debug.Log(rangeChecks.Length);
        //if (rangeChecks.Length > 0)
        //{
        //    Transform target = rangeChecks[0].transform;
        //    Debug.Log(target.name);
        //    if (target.CompareTag("Pato"))
        //    {
        //        animalTarget = target; // ponemos el pato como objetivo
        //        // Acceder a la variable aSalvo de Pato
        //        estaASalvo = animalTarget.GetComponentInParent<Pato>().aSalvo;
        //        Debug.Log("estaASalvo: " + estaASalvo);

        //        Vector3 directionToTarget = (target.position - transform.position).normalized;
        //        float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
        //        float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

        //        if (dotProduct > angleThreshold)
        //        {
        //            float distanciaToTarget = Vector3.Distance(transform.position, target.position);

        //            if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
        //            {
        //                // Verificar si el tag es "Castor" o "Pato"
        //                if (estaASalvo == false)
        //                {
        //                    puedeVer = true;
        //                    return ChaseState.Finished;
        //                }
        //                else
        //                {
        //                    puedeVer = false;
        //                    return ChaseState.Failed;
        //                }
        //            }
        //            else
        //            {
        //                puedeVer = false;
        //                return ChaseState.Failed;
        //            }
        //        }
        //        else
        //        {
        //            puedeVer = false;
        //            return ChaseState.Failed;
        //        }
        //    }           
        //}
        //return ChaseState.Failed;
        */
    }

    public ChaseState HayHuevos()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioHuevos, targetMaskHuevos);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

            if (dotProduct > angleThreshold)
            {
                float distanciaToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanciaToTarget, obstructionMask))
                {
                    puedeVerArena = true;
                    return ChaseState.Finished;

                }
                else
                {
                    puedeVerArena = false;
                    return ChaseState.Failed;

                }
            }
            else
            {
                puedeVerArena = false;
                return ChaseState.Failed;
            }
        }
        else if (puedeVerArena)
        {
            puedeVerArena = false;
            return ChaseState.Failed;
        }
        return ChaseState.Failed;
    }

    public ChaseState HayArena()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radioHuevos, targetMaskArena);

        if (rangeChecks.Length > 0)
        {
            Transform target = rangeChecks[0].transform;
            sandTarget = target;

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
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

    //Acci�n perseguir animales
   public ChaseState Chase()
{
    GameObject targetParent = animalTarget.gameObject.transform.parent.gameObject;
    var castor = targetParent.GetComponent<Castor>();
    var pato = targetParent.GetComponent<Pato>();

    if (animalTarget == null)
    {
        Debug.Log("Chase failed: no target.");
        return ChaseState.Failed; // No hay objetivo
    }

    if(castor.aSalvo){ //MUUUUUUUUUUUUUUY IMPORTANTE METER AQUI TAMBIEN AL PATO EHHHH IMPORTANTE IMPORTANTE HACER TAMBIEN pato.aSalvo EEEEEEH IMPORTANTE MIRENME
        return ChaseState.Finished;
    }
    float minDist = crocNav.stoppingDistance;
    float dist = Vector3.Distance(animalTarget.position, transform.position);

    Debug.Log($"Chasing target. Current distance: {dist}, Minimum distance: {minDist}");

    if (dist <= 2.0)
    {
        Debug.Log("Chase finished: target reached.");
        return ChaseState.Finished; // Se ha llegado al objetivo
    }

    if (crocNav.pathPending || crocNav.remainingDistance > minDist)
    {
        crocNav.SetDestination(animalTarget.position); // Actualiza el destino
        return ChaseState.Enproceso; // La persecución está en curso
    }

    Debug.Log("Chase finished: close enough to target.");
    return ChaseState.Finished; // Se ha llegado suficientemente cerca
}

    //Acci�n comer tanto huevos como animales
    public void Eat(bool animal, bool eggs)
    {
    if (animal && animalTarget != null) // Si vamos a comer un animal y hay un objetivo animal
    {
        // Obtener el padre del GameObject que contiene los componentes asociados al objetivo animal
        GameObject targetParent = animalTarget.gameObject.transform.parent.gameObject;
        var castor = targetParent.GetComponent<Castor>();
        var pato = targetParent.GetComponent<Pato>();
        if (castor != null)
        {
            if(!castor.aSalvo){  
            // Destruir el GameObject del animal y su padre
            GameObject.Destroy(targetParent);
            // Restablecer la cantidad de hambre a cero
            hambre = 0;
            }
            
        }
        if (pato != null)
        {
            if(!pato.aSalvo){
            // Destruir el GameObject del animal y su padre
            GameObject.Destroy(targetParent);
            // Restablecer la cantidad de hambre a cero
            hambre = 0;
            }
            
        }
        
    }
    else if (eggs && eggsTarget != null) // Si vamos a comer huevos y hay un objetivo de huevos
    {
        // Obtener el padre del GameObject que contiene los componentes asociados al objetivo de huevos
        GameObject targetParent = eggsTarget.gameObject.transform.parent.gameObject;

        // Destruir el GameObject de los huevos y su padre
        GameObject.Destroy(targetParent);

        // Aumentar la energía
        energia += 20;
        energia = Mathf.Clamp(energia, 0f, 100f);
    }

    
    }

    //Acci�n huir a arena
    public ChaseState irArena()
    {
        if (sandTarget != null)
        {
            crocNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            crocNav.speed = crocNav.speed + 5f;
            energia -= 5;
            energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                Debug.Log("COCODRILO EN ARENA");
                miedo = 0; //reducimos el miedo
                crocNav.isStopped = true;//paramos el movimiento
                StartCoroutine(ReanudarMovimiento());
                crocNav.speed = crocNav.speed - 5f;
                return ChaseState.Finished;
            }
            crocNav.speed = crocNav.speed -5f;
            return ChaseState.Enproceso;// se ha llegado al punto indicado aunque el animal ya no este (muerto o escondido)

        }
        else
        {
            crocNav.stoppingDistance = 0;
            crocNav.speed--;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }
    

    public IEnumerator EsperarLlegada()
    {
        yield return new WaitUntil(() => crocNav.remainingDistance <= crocNav.stoppingDistance); //esperar a que el pato llegue a su destino
    }

    public IEnumerator ReanudarMovimiento()
    {
        yield return new WaitForSeconds(5f);
        crocNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }

    public ChaseState IrAHuevos()
    {
        float stopDistance = crocNav.stoppingDistance;
        crocNav.stoppingDistance = 0;
        float minDist = crocNav.stoppingDistance;
        if (eggsTarget != null)
        {
            float dist = Vector3.Distance(eggsTarget.position, transform.position);

            if (dist > minDist)
            {

                crocNav.SetDestination(eggsTarget.position); //se pone como punto de destino la posicion de los huevos

            }

            return ChaseState.Finished;// se ha llegado al punto indicado 

        }
        else
        {
            crocNav.stoppingDistance = stopDistance;
            return ChaseState.Failed; //no haya animal al que perseguir
        }
    }
    #endregion

    #region "Otros"
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
            HayCaza();
        }
    }
    #endregion
}
