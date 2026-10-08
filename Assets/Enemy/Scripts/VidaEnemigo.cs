using UnityEngine;

public class VidaEnemigo : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 100;

    private int vidaActual;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;

        Debug.Log(
            gameObject.name +
            " recibió " +
            cantidad +
            " de daño. Vida restante: " +
            vidaActual
        );

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log(gameObject.name + " murió.");

        Destroy(gameObject);
    }
}