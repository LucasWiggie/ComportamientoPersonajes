using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bark : MonoBehaviour
{
    public Sprite[] images; // Array de im�genes
    private Image imageComponent; // Referencia al componente Image
    private Quaternion initialRotation; // Almacenar la rotaci�n inicial

    void Start()
    {
        // Obtener el componente Image del objeto donde est� este script
        imageComponent = GetComponent<Image>();
        // Guardar la rotaci�n inicial del objeto
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Mantener la rotaci�n del objeto fija en la rotaci�n inicial
        transform.rotation = initialRotation;
    }

    // M�todo para cambiar la imagen basado en el �ndice
    public void ChangeImage(int index)
    {
        if (index >= 0 && index < images.Length)
        {
            imageComponent.sprite = images[index];
        }
        else
        {
            Debug.LogWarning("�ndice fuera de rango");
        }
    }
}
