using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    void Start()
    {
        for (int i = 0; i < nCocodrilos; i++)
        {
            Instantiate(cocodrilo, new Vector3(Random.Range(-13.0f, 13.0f), 2.77f, Random.Range(-13.0f, 13.0f)), new Quaternion(90f, 0f, 0f, -90f));
        }

        for (int i = 0; i < nCastores; i++)
        {
            Instantiate(castor, new Vector3(Random.Range(-13.0f, 13.0f), 2.74f, Random.Range(-13.0f, 13.0f)), new Quaternion(90f, 0f, 0f, -90f));
        }

        for (int i = 0; i < nSalamandras; i++)
        {
            Instantiate(salamandra, new Vector3(Random.Range(-13.0f, 13.0f), -0.26f, Random.Range(-13.0f, 13.0f)), new Quaternion(0f, 0f, 0f, -90f));
        }

        for (int i = 0; i < nPatos; i++)
        {
            Instantiate(pato, new Vector3(Random.Range(-9.0f, 9.0f), -0.4f, Random.Range(-13.0f, 13.0f)), new Quaternion(0f, 0f, 0f, -90f));
        }

        for (int i = 0; i < nPatosHijos; i++)
        {
            Instantiate(patoHijo, new Vector3(Random.Range(-9.0f, 9.0f), -0.4f, Random.Range(-9.0f, 9.0f)), new Quaternion(0f, 0f, 0f, -90f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
