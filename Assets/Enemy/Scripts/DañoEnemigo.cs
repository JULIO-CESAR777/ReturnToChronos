using UnityEngine;

public class DañoEnemigo: MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private int daño = 10;

    private void OnTriggerEnter(Collider otro)
    {
        VidaJugador vidaJugador =
            otro.GetComponentInParent<VidaJugador>();

        if (vidaJugador == null)
            return;

        vidaJugador.RecibirDanio(
            daño,
            transform.position
        );
    }
}