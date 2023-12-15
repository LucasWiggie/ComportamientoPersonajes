using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GeneracionAleatoria : MonoBehaviour
{

    public GameObject cocodrilo;
    public GameObject castor;
    public GameObject salamandra;
    public GameObject pato;
    public GameObject patoHijo;

    public int nCocodrilos;
    public int nCastores;
    public int nSalamandras;
    public int nPatos;
    public int nPatosHijos;


    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < nCocodrilos; i++)
        {
            NavMeshHit hit;
            Vector3 randomPoint = GetRandomPointOnNavMesh(2.77f);
            while (!NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                 randomPoint = GetRandomPointOnNavMesh(2.7f);
            }

            Instantiate(cocodrilo, hit.position, new Quaternion(90f, 0f, 0f, -90f));

        }

        for (int i = 0; i < nCastores; i++)
        {
            NavMeshHit hit;
            Vector3 randomPoint = GetRandomPointOnNavMesh(2.7f);
            while (!NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                randomPoint = GetRandomPointOnNavMesh(2.7f);
            }

            Instantiate(castor, hit.position, new Quaternion(90f, 0f, 0f, -90f));
        }

        for (int i = 0; i < nSalamandras; i++)
        {
            NavMeshHit hit;
            Vector3 randomPoint = GetRandomPointOnNavMesh(2.63f); //No bajar de 2.63 que se cuelga
            while (!NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
            {
                randomPoint = GetRandomPointOnNavMesh(2.63f);//No bajar de 2.63 que se cuelga
            }

            
            GameObject instance = Instantiate(salamandra, hit.position, new Quaternion(0f, 0f, 0f, 0f));
            instance.transform.position = new Vector3(hit.position.x, -0.15f, hit.position.z);
        }

        for (int i = 0; i < nPatos; i++)
        {
            NavMeshHit hit;
            Vector3 randomPoint = GetRandomPointOnNavMeshPato(2.63f);//No bajar de 2.63 que se cuelga
            while (!NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
            {
                randomPoint = GetRandomPointOnNavMeshPato(2.63f);//No bajar de 2.63 que se cuelga
            }

            GameObject instance = Instantiate(pato, hit.position, new Quaternion(0f, 0f, 0f, 0f));
            instance.transform.position = new Vector3(hit.position.x, -0.39f, hit.position.z);
        }

    }

    Vector3 GetRandomPointOnNavMesh(float yOffset)
    {
        return new Vector3(Random.Range(-13.0f, 13.0f), yOffset, Random.Range(-13.0f, 13.0f));
    }

    Vector3 GetRandomPointOnNavMeshPato(float yOffset)
    {
        return new Vector3(Random.Range(-9.0f, 9.0f), yOffset, Random.Range(-13.0f, 13.0f));
    }

    // Update is called once per frame
    void Update()
    {

    }
}

