using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 100;

    [Header("UI")]
    [SerializeField] private Image imagenVida;

    
    
    [Header("Daño")]
    [SerializeField] private float tiempoInvulnerabilidad = 0.6f;

    [Header("Knockback")]
    [SerializeField] private float fuerzaKnockback = 8f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent alRecibirDanio;
    [SerializeField] private UnityEvent alMorir;

    private int vidaActual;
    private bool invulnerable;

    private ControladorJugador controladorJugador;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaInvulnerable => invulnerable;

    private void Awake()
    {
        vidaActual = vidaMaxima;

        controladorJugador =
            GetComponent<ControladorJugador>();

        ActualizarBarraVida();
    }

    public void RecibirDanio(
        int cantidad,
        Vector3 posicionAtacante
    )
    {
        if (invulnerable)
            return;

        if (cantidad <= 0)
            return;

        vidaActual -= cantidad;

        vidaActual =
            Mathf.Max(vidaActual, 0);
        
        ActualizarBarraVida();

        Debug.Log(
            "Player recibió " +
            cantidad +
            " de daño. Vida: " +
            vidaActual +
            "/" +
            vidaMaxima
        );

        // ============================================
        // KNOCKBACK
        // ============================================

        if (controladorJugador != null)
        {
            Vector3 direccion =
                transform.position -
                posicionAtacante;

            direccion.y = 0f;

            if (direccion.sqrMagnitude < 0.01f)
            {
                direccion =
                    -transform.forward;
            }

            controladorJugador.AplicarKnockback(
                direccion.normalized,
                fuerzaKnockback
            );
        }

        alRecibirDanio?.Invoke();

        if (vidaActual <= 0)
        {
            Morir();
            return;
        }

        StartCoroutine(
            InvulnerabilidadTemporal()
        );
    }

    
    private void ActualizarBarraVida()
    {
        if (imagenVida == null)
            return;

        imagenVida.fillAmount =
            (float)vidaActual / vidaMaxima;
    }
    
    private IEnumerator InvulnerabilidadTemporal()
    {
        invulnerable = true;

        yield return new WaitForSeconds(
            tiempoInvulnerabilidad
        );

        invulnerable = false;
    }

    private void Morir()
    {
        Debug.Log("Player murió.");

        alMorir?.Invoke();

        // Después aquí podemos poner:
        // animación
        // respawn
        // Game Over
        // etc.
    }
}