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
    public GameObject palo;
    public GameObject mosca;

    public int nCocodrilos;
    public int nCastores;
    public int nSalamandras;
    public int nPatos;
    public int nPalos;
    public int nMoscas;

    void Awake()
    {
        // Puedes dejar este bloque vacío si no quieres que se generen animales automáticamente al inicio
    }

    public void GenerateAnimals()
    {
        ClearAnimals();

        for (int i = 0; i < nCocodrilos; i++)
        {
            InstantiateAnimal(cocodrilo);
        }

        for (int i = 0; i < nCastores; i++)
        {
            InstantiateAnimal(castor);
        }

        for (int i = 0; i < nSalamandras; i++)
        {
            InstantiateAnimal(salamandra, -0.35f);
        }

        for (int i = 0; i < nPatos; i++)
        {
            InstantiateAnimal(pato, 1.48f);
        }

        for (int i = 0; i < nPalos; i++)
        {
            InstantiateAnimal(palo);
        }

        for (int i = 0; i < nMoscas; i++)
        {
            InstantiateAnimal(mosca, 1f);
        }
    }

    private void InstantiateAnimal(GameObject animalPrefab, float yOffset = 0f)
    {
        NavMeshHit hit;
        Vector3 randomPoint = GetRandomPointOnNavMesh(2.77f);
        while (!NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            randomPoint = GetRandomPointOnNavMesh(2.77f);
        }

        GameObject instance = Instantiate(animalPrefab, hit.position, Quaternion.identity);
        instance.transform.position = new Vector3(hit.position.x, hit.position.y + yOffset, hit.position.z);
    }

    public void ClearAnimals()
    {
        DestroyAnimalsWithTag("Castor");
        DestroyAnimalsWithTag("Pato");
        DestroyAnimalsWithTag("Cocodrilo");
        DestroyAnimalsWithTag("Salamandra");
        DestroyAnimalsWithTag("Palo");
        DestroyAnimalsWithTag("Mosca");
        DestroyAnimalsWithTag("Huevo");
    }

    private void DestroyAnimalsWithTag(string tag)
    {
        foreach (GameObject animal in GameObject.FindGameObjectsWithTag(tag))
        {
            Destroy(animal);
        }
    }

    Vector3 GetRandomPointOnNavMesh(float yOffset)
    {
        return new Vector3(Random.Range(-28.0f, 28.0f), yOffset, Random.Range(-28.0f, 28.0f));
    }

    void Update()
    {
        // Dejar vacío si no hay necesidad de actualizar cada frame
    }
}
