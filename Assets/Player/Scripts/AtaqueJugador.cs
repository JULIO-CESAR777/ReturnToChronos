using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtaqueJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private JugadorVisual visual;
    [SerializeField] private Transform attackPoint;

    [Header("Ataque")]
    [SerializeField] private float tiempoEntreAtaques = 0.5f;

    [Header("Hitbox")]
    [SerializeField] private float radioAtaque = 0.7f;
    [SerializeField] private LayerMask capasAtacables;
    [SerializeField] private int daño = 25;

    private float siguienteAtaque;

    private void Awake()
    {
        if (visual == null)
        {
            visual = GetComponentInChildren<JugadorVisual>();
        }
    }

    private void Update()
    {
        if (!ControladorJugador.SePulsoAtaque())
            return;

        IntentarAtacar();
    }

    private void IntentarAtacar()
    {
        if (Time.time < siguienteAtaque)
            return;

        siguienteAtaque = Time.time + tiempoEntreAtaques;

        if (visual != null)
        {
            visual.ReproducirAtaque();
        }
    }

    public void AplicarGolpe()
    {
        if (attackPoint == null)
            return;

        Collider[] objetivos = Physics.OverlapSphere(
            attackPoint.position,
            radioAtaque,
            capasAtacables
        );

        // Sets para evitar procesar el mismo objetivo múltiples veces en un solo ataque
        HashSet<VidaEnemigo> enemigosProcesados = new HashSet<VidaEnemigo>();
        HashSet<LuzPuzzle> lucesProcesadas = new HashSet<LuzPuzzle>();

        foreach (Collider objetivo in objetivos)
        {
            // ENEMIGOS
            VidaEnemigo enemigo = objetivo.GetComponentInParent<VidaEnemigo>();
            if (enemigo != null && !enemigosProcesados.Contains(enemigo))
            {
                enemigosProcesados.Add(enemigo);
                enemigo.RecibirDaño(daño);
            }

            // PUZZLE
            LuzPuzzle luzPuzzle = objetivo.GetComponentInParent<LuzPuzzle>();
            if (luzPuzzle != null && !lucesProcesadas.Contains(luzPuzzle))
            {
                lucesProcesadas.Add(luzPuzzle);
                luzPuzzle.RecibirGolpe();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            radioAtaque
        );
    }
}