using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Patitos : MonoBehaviour
{

    private NavMeshAgent patitoNav;
    // Start is called before the first frame update
    void Start()
    {
        patitoNav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void  FixedUpdate()
    {
        movimientoAleatorio();
    }

    private void movimientoAleatorio()
    {
        if (Time.time >= nextRandomMovementTime)
        {
            Vector3 randomPoint = RandomNavmeshLocation(60f); // Obtener un punto aleatorio en el NavMesh
            patitoNav.SetDestination(randomPoint); // Establecer el punto como destino
            //Debug.Log("pato se mueve");
            nextRandomMovementTime = Time.time + movementInterval; // Actualizar el tiempo para el próximo movimiento
        }
    }

    // Variables para controlar el intervalo de movimiento
    private float nextRandomMovementTime = 0f;
    public float movementInterval = 3f;

    // Función para encontrar un punto aleatorio en el NavMesh dentro de un radio dado
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
}
