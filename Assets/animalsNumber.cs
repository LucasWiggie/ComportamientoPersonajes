using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class animalsNumber : MonoBehaviour
{
    public TextMeshProUGUI numCastor;
    public TextMeshProUGUI numPato;
    public TextMeshProUGUI numCroc;
    public TextMeshProUGUI numSal;
    private int valueCastor;
    private int valuePato;
    private int valueCroc;
    private int valueSal;

    public Button sumarCastor;
    public Button sumarPato;
    public Button sumarCroc;
    public Button sumarSal;
    public Button restarCastor;
    public Button restarPato;
    public Button restarCroc;
    public Button restarSal;
    // Start is called before the first frame update
    void Start()
    {
        sumarCastor.onClick.AddListener(() => Suma(numCastor, ref valueCastor));
        sumarPato.onClick.AddListener(() => Suma(numPato, ref valuePato));
        sumarCroc.onClick.AddListener(() => Suma(numCroc, ref valueCroc));
        sumarSal.onClick.AddListener(() => Suma(numSal, ref valueSal));

        restarCastor.onClick.AddListener(() => Resta(numCastor, ref valueCastor));
        restarPato.onClick.AddListener(() => Resta(numPato, ref valuePato));
        restarCroc.onClick.AddListener(() => Resta(numCroc, ref valueCroc));
        restarSal.onClick.AddListener(() => Resta(numSal, ref valueSal));
    }



    public void Suma(TextMeshProUGUI number, ref int value)
    {
        value++;
        number.SetText(value.ToString());
    }

    public void Resta(TextMeshProUGUI number,ref int value)
    {
        if (value > 0)
        {
            value--;
        }
        number.SetText(value.ToString());
    }
}
