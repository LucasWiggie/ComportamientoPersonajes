using UnityEngine;

public class Palo : MonoBehaviour
{
    // Referencia al castor que ha reservado este palo
    public Castor castorReservante; // Referencia al castor que ha recogido este palo

    // Indica si el palo está siendo recogido por otro castor
    public bool siendoRecogido = false;

    // Método para que el palo realice acciones cuando es recogido por un castor
    public void RecogidoPorCastor()
    {
        // Aquí puedes realizar cualquier acción necesaria antes de destruir el palo
        // Por ejemplo, desactivar cualquier comportamiento del palo, reproducir un efecto de sonido, etc.
        
        // Luego, destruye el palo
        Destroy(gameObject);
    }
}