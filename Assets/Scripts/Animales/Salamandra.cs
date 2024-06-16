using MBT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Salamandra : MonoBehaviour
{
    public float radio;
    [Range(0, 360)]
    public float angulo;
    public GameObject playerRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public LayerMask targetMaskHuevos;
    public LayerMask targetMaskArena;
    public LayerMask huirPato;
    public GameObject huevoPrefab; // Prefab de los huevos
    public bool puedeVer;

    public float hambre; //Rango 0-100 las 3
    public float energia;
    public float miedo;
    public float temorHuevos;
    public int moscasComidas;
    private float distMinHuevo = 3.0f;

    public enum ChaseState
    {
        Finished,
        Failed,
        Enproceso
    }

    // BTs
    // BTs de cada accion
    public GameObject btHambre;
    public GameObject btEnergia;
    public GameObject btMiedoPatos;
    public GameObject btPonerHuevos;

    //Bool bts
    private bool boolHambre = false;
    private bool boolEnergia = false;
    private bool boolMiedoPato = false;
    public bool boolProtegerHuevos = false;
    private bool boolPonerHuevos = false;

    //Utilidades
    public float uHambre;
    public float uEnergia;
    public float uMiedo;
    public float uTemorHuevos;
    public int uMoscasComidas;

    float hambreRate = 1f;
    float energiaRate = 1f;

    public bool isDefaultMov = true;

    public bool aSalvo = false;
    private bool descansando = false;
    public bool poniendoHuevos = false;
    private bool comeMosca = false;

    private float tiempoInicioPonerHuevos = 0f;
    private const float tiempoPonerHuevos = 4f;
    public bool huevosPuestos = false;


    private NavMeshAgent salamandraNav;

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 5f;

    // Objetivos
    private Transform eggsTarget;//huevos objetivo
    private Transform sandTarget;//arena
    private Transform moscaTarget;
    public Transform huevoAProteger;

    //Peligros
    private List<Transform> patosCercanos = new List<Transform>();
    public float distanciaMaxima = 25;

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
    public float getTemorHuevos()
    {
        return this.temorHuevos;
    }
    public int getMoscasComidas()
    {
        return this.moscasComidas;
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
    public void setTemorHuevos(float t)
    {
        this.temorHuevos = t;
    }
    public void setMoscasComidas(int t)
    {
        this.moscasComidas = t;
    }

    private void Start()
    {
        patosCercanos.Clear();
        playerRef = this.gameObject;
        salamandraNav = GetComponent<NavMeshAgent>();
        //InvokeRepeating("NuevoDestinoAleatorio", 0f, movementInterval);

        hambre = 30;
        energia = 80;
        miedo = 0;
        temorHuevos = 0;
        moscasComidas = 0;

        uHambre = hambre;
        uEnergia = energia;
        uMiedo = miedo;
        uTemorHuevos = temorHuevos;
        uMoscasComidas = moscasComidas;

    }

    private void Update()
    {
        UpdateVariables();
        DetectarPatosCercanos();
        ActualizarMiedo();

        if (moscaTarget != null)
        {
            if (transform.position.x == moscaTarget.position.x && transform.position.z == moscaTarget.position.z && !comeMosca)
            {
                comeMosca = true;
            }
        }
    }

    private void FixedUpdate()
    {
        UtilitySystem();
        if (boolHambre)
        {
            btHambre.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolEnergia)
        {
            btEnergia.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolMiedoPato)
        {
            btMiedoPatos.GetComponent<MonoBehaviourTree>().Tick();
        }
        else if (boolProtegerHuevos)
        {
            ProtegerHuevo();
        }
        else if (boolPonerHuevos)
        {
            btPonerHuevos.GetComponent<MonoBehaviourTree>().Tick();
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
            salamandraNav.SetDestination(randomPoint); // Establecer el punto como destino
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

    public void UtilitySystem()
    {
        uHambre = this.getHambre();
        uEnergia = this.getEnergia();
        uMiedo = this.getMiedo();
        uTemorHuevos = this.getTemorHuevos();
        uMoscasComidas = this.getMoscasComidas();

        if (uHambre >= 100 || uEnergia <= 0)
        {
            Debug.Log(this.gameObject + " ha muerto");
            Destroy(this.gameObject);
        }
        else if (uMiedo > 50) 
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = true;
            boolProtegerHuevos = false;
            boolPonerHuevos = false;
            aSalvo = false;
            huevosPuestos = false;

            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(true);
            btPonerHuevos.SetActive(false);
        }
        else if (uTemorHuevos > 60)
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = true;
            boolPonerHuevos = false;
            aSalvo = false;
            huevosPuestos = false;

            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btPonerHuevos.SetActive(false);
        }
        else if (uEnergia < 20 || descansando)
        {
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = true;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            boolPonerHuevos = false;
            aSalvo = false;
            huevosPuestos = false;

            btHambre.SetActive(false);
            btEnergia.SetActive(true);
            btMiedoPatos.SetActive(false);
            btPonerHuevos.SetActive(false);
        }
        else if (uHambre > 70)
        {
            isDefaultMov = false;
            boolHambre = true;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            boolPonerHuevos = false;
            aSalvo = false;
            huevosPuestos = false;

            btHambre.SetActive(true);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btPonerHuevos.SetActive(false);
        }
        else if (uMoscasComidas >= 5)
        {
            // A poner huevos
            isDefaultMov = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            boolPonerHuevos = true;
            aSalvo = false;
            huevosPuestos = false;

            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btPonerHuevos.SetActive(true);
        }
        else if(!boolProtegerHuevos)
        {
            isDefaultMov = true;

            aSalvo = false;
            boolHambre = false;
            boolEnergia = false;
            boolMiedoPato = false;
            boolProtegerHuevos = false;
            boolPonerHuevos = false;
            huevosPuestos = false;

            btHambre.SetActive(false);
            btEnergia.SetActive(false);
            btMiedoPatos.SetActive(false);
            btPonerHuevos.SetActive(false);
        }
    }

    private void UpdateVariables()
    {
        hambre += hambreRate * Time.deltaTime;
        energia -= energiaRate * Time.deltaTime;

        hambre = Mathf.Clamp(hambre, 0f, 100f);
        energia = Mathf.Clamp(energia, 0f, 100f);
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

    public ChaseState HayArena()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMaskArena);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

                if (dotProduct > angleThreshold)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        if (distanceToTarget < closestDistance)
                        {
                            closestDistance = distanceToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar la presa m�s cercana como el objetivo
                sandTarget = closestTarget;
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

    public ChaseState IrArena()
    {
        if (sandTarget != null)
        {
            salamandraNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            //salamandraNav.speed = salamandraNav.speed + 5f;
            //energia -= 0.05f;
            //energia = Mathf.Clamp(energia, 0f, 100f);

            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                aSalvo = true;

                // Incrementa la energ�a del castor mientras est� en la presa
                energia += 0.08f;

                // Permitir que el castor salga de la presa solo si su energ�a es >= 90
                descansando = true;
                if (energia > 80)//una cantidad necesaria de energia que reponer para poder salir del nenufar, evitando cambios de comportamiento por cte por el cambio del valor de energia en la franja de cansancio
                {
                    descansando = false;
                    return ChaseState.Finished;
                }

                return ChaseState.Enproceso;

                //salamandraNav.speed = salamandraNav.speed - 5f;
            } 
            else
            {
                if (miedo > 70)
                {
                    energia -= 0.02f;
                    salamandraNav.speed += 0.003f;
                }
            }
            //salamandraNav.speed = salamandraNav.speed - 5f;
            return ChaseState.Enproceso;
        }
        else
        {
            salamandraNav.stoppingDistance = 0;
            salamandraNav.speed--;
            return ChaseState.Failed; 
        }
    }

    public ChaseState HayMosca()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radio, targetMask);
        //Debug.Log("Number of objects found: " + rangeChecks.Length);

        if (rangeChecks.Length > 0)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in rangeChecks)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angleThreshold = Mathf.Cos(Mathf.Deg2Rad * (angulo / 2));

                if (dotProduct > angleThreshold)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        if (distanceToTarget < closestDistance)
                        {
                            closestDistance = distanceToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }

            if (closestTarget != null)
            {
                // Asignar la presa más cercana como el objetivo
                moscaTarget = closestTarget;
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

    public ChaseState IrMosca()
    {
        salamandraNav.stoppingDistance = 0;

        if (moscaTarget != null)
        {
            salamandraNav.SetDestination(moscaTarget.position);
            //StartCoroutine(EsperarLlegada());
            if (comeMosca)
            {
                return ChaseState.Finished;
            }
            return ChaseState.Enproceso;
        }
        else
        {
            //HayMosca();
            return ChaseState.Failed;
        }
    }

    public ChaseState ComerMosca()
    {
        if (comeMosca)
        {
            Destroy(moscaTarget.gameObject);
            moscaTarget = null;
            moscasComidas++;
            hambre -= 20;

            return ChaseState.Finished;
        }
        else
        {
            return ChaseState.Failed;
        }
    }

    public ChaseState PonerHuevos()
    {
        if (sandTarget != null)
        {
            salamandraNav.SetDestination(sandTarget.position); //se pone como punto de destino la posicion de la arena
            //salamandraNav.speed = salamandraNav.speed + 5f;
            //energia -= 0.05f;
            //energia = Mathf.Clamp(energia, 0f, 100f);

            // Si está en la arena
            if (transform.position.x == sandTarget.position.x && transform.position.z == sandTarget.position.z)
            {
                Debug.Log("Salamandra en arena");
                aSalvo = true;
                poniendoHuevos = true;

                if (Time.time - tiempoInicioPonerHuevos >= tiempoPonerHuevos) // Si han pasado los 4 segundos
                {
                    if (!huevosPuestos)
                    {
                        InstanciarHuevos();
                    }

                    poniendoHuevos = false;
                    moscasComidas = 0;
                    return ChaseState.Finished;
                }

                return ChaseState.Enproceso;

                //salamandraNav.speed = salamandraNav.speed - 5f;
            }
            else
            {
                tiempoInicioPonerHuevos = Time.time; // reestablecemos el tiempo en el que se ponen los huevos
                //if (miedo > 70)
                //{
                //    energia -= 0.02f;
                //    salamandraNav.speed += 0.003f;
                //}
            }
            //salamandraNav.speed = salamandraNav.speed - 5f;
            return ChaseState.Enproceso;
        }
        else
        {
            salamandraNav.stoppingDistance = 0;
            salamandraNav.speed--;
            return ChaseState.Failed;
        }
    }

    void DetectarPatosCercanos()
    {
        patosCercanos.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, distanciaMaxima, huirPato);
        foreach (Collider collider in colliders)
        {
            patosCercanos.Add(collider.transform);
        }
    }

    void ActualizarMiedo()
    {
        miedo = 0f;

        foreach (Transform objetivo in patosCercanos)
        {
            float distancia = Vector3.Distance(transform.position, objetivo.position);
            float miedoIncremental = Mathf.Clamp01(1 - distancia / distanciaMaxima) * 100; // Calcular el miedo incremental normalizado

            miedo = Mathf.Max(miedo, miedoIncremental); // Mantener el mayor valor de miedo
        }
    }

    public void ProtegerHuevo()
    {
        if (huevoAProteger != null)
        {
            float distanceToEgg = Vector3.Distance(transform.position, huevoAProteger.position);
            if (distanceToEgg > distMinHuevo)
            {
                salamandraNav.SetDestination(huevoAProteger.position);
                aSalvo = false;
            }
            else
            {
                aSalvo = true;
            }
            huevoAProteger.GetComponentInParent<Huevo>().aSalvo = aSalvo;
        }
        else
        {
            // Opcional: Lógica adicional si no hay un huevo a proteger
            salamandraNav.stoppingDistance = 0;
            salamandraNav.speed--;
        }

        //isDefaultMov = false;
        //if (huevoAProteger != null)
        //{
        //    salamandraNav.SetDestination(huevoAProteger.position); //se pone como punto de destino la posicion de la arena

        //    if (Vector3.Distance(transform.position, huevoAProteger.position) < distMinHuevo)
        //    {
        //        aSalvo = true;
        //        huevoAProteger.GetComponentInParent<Huevo>().aSalvo = true;
        //    } 
        //    else
        //    {
        //        Debug.Log("Entra al else para poner el aSalvo del huevo a false");
        //        huevoAProteger.GetComponentInParent<Huevo>().aSalvo = false;
        //    }
        //}
        //else
        //{
        //    salamandraNav.stoppingDistance = 0;
        //    salamandraNav.speed--;
        //}
    }

    public IEnumerator ReanudarMovimiento()
    {
        yield return new WaitForSeconds(7f);
        Debug.Log("Reanudamos movimiento");
        salamandraNav.isStopped = false; //reanudamos el movimiento despues de x segundos
    }

    public IEnumerator RecuperarEnergia()
    {
        yield return new WaitForSeconds(7f);
        Debug.Log("Salamandra ha recuperado energ�a");
        energia = 100;
    }

    private void InstanciarHuevos()
    {
        Vector3 posicionHuevos = new Vector3(transform.position.x + 0.5f, transform.position.y + 1.75f, transform.position.z + 0.5f); // Ajustar esta posición según sea necesario
        GameObject huevoInstanciado = Instantiate(huevoPrefab, posicionHuevos, Quaternion.identity);
        Huevo huevoScript = huevoInstanciado.GetComponent<Huevo>();
        huevoScript.madreSalamandra = this;
        huevoScript.transformMadreSalamandra = transform;
        huevosPuestos = true;
    }
}

