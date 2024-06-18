using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bark : MonoBehaviour
{
    public Sprite[] images; // Array de imágenes
    private Image imageComponent; // Referencia al componente Image
    private Quaternion initialRotation; // Almacenar la rotación inicial

    void Start()
    {
        // Obtener el componente Image del objeto donde está este script
        imageComponent = GetComponent<Image>();
        // Guardar la rotación inicial del objeto
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Mantener la rotación del objeto fija en la rotación inicial
        transform.rotation = initialRotation;
    }

    // Método para cambiar la imagen basado en el índice
    public void ChangeImage(int index)
    {
        if (index >= 0 && index < images.Length)
        {
            imageComponent.sprite = images[index];
        }
        else
        {
            Debug.LogWarning("Índice fuera de rango");
        }
    }
}
