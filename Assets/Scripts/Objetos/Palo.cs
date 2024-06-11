using UnityEngine;

public class Palo : MonoBehaviour
{
    // Referencia al castor que ha reservado este palo
    public Castor Reservado { get; private set; }

    // Método para reservar el palo
    public bool Reservar(Castor castor)
    {
        if (Reservado == null)
        {
            Reservado = castor;
            return true;
        }
        return false;
    }

    // Método para verificar si está reservado
    public bool EstaReservado()
    {
        return Reservado != null;
    }
}