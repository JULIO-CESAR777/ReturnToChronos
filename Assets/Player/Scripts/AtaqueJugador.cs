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
    private EfectosJugador efectos;

    private void Awake()
    {
        efectos = GetComponent<EfectosJugador>();
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
            efectos?.SonarAtaque();
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

        // Estructuras para evitar registrar múltiples golpes al mismo objeto en un mismo frame
        HashSet<LuzPuzzle> lucesProcesadas = new HashSet<LuzPuzzle>();
        HashSet<VidaEnemigo> enemigosProcesados = new HashSet<VidaEnemigo>();

        bool huboImpacto = false;
        bool huboRemate = false;
        foreach (Collider objetivo in objetivos)
        {
            // PUZZLE
            LuzPuzzle luzPuzzle = objetivo.GetComponentInParent<LuzPuzzle>();
            if (luzPuzzle != null && !lucesProcesadas.Contains(luzPuzzle))
            {
                lucesProcesadas.Add(luzPuzzle);
                luzPuzzle.RecibirGolpe();
            }

            // ENEMIGOS
            VidaEnemigo enemigo = objetivo.GetComponentInParent<VidaEnemigo>();
            if (enemigo != null && !enemigosProcesados.Contains(enemigo))
            {
                enemigosProcesados.Add(enemigo);
                if (enemigo.EstaMuerto || daño <= 0) continue;
                Vector3 punto = objetivo.bounds.ClosestPoint(attackPoint.position);
                enemigo.RecibirDaño(daño);
                huboImpacto = true;
                huboRemate |= enemigo.EstaMuerto;
                efectos?.Chispas(punto, enemigo.EstaMuerto);
            }
        }
        if (huboImpacto) efectos?.ConfirmarGolpe(huboRemate);
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