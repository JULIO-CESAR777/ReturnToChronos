using UnityEngine;

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
            visual =
                GetComponentInChildren<JugadorVisual>();
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

        siguienteAtaque =
            Time.time + tiempoEntreAtaques;

        if (visual != null)
        {
            visual.ReproducirAtaque();
        }
    }

    // =========================================================
    // ESTE MÉTODO LO LLAMAREMOS DESDE LA ANIMACIÓN
    // =========================================================

    public void AplicarGolpe()
    {
        if (attackPoint == null)
            return;

        Collider[] objetivos =
            Physics.OverlapSphere(
                attackPoint.position,
                radioAtaque,
                capasAtacables
            );

        foreach (Collider objetivo in objetivos)
        {
            // =====================================================
            // ENEMIGO
            // =====================================================

            VidaEnemigo enemigo =
                objetivo.GetComponentInParent<VidaEnemigo>();

            if (enemigo != null)
            {
                enemigo.RecibirDaño(daño);
            }


            // =====================================================
            // PUZZLE
            // =====================================================

            LuzPuzzle luzPuzzle =
                objetivo.GetComponentInParent<LuzPuzzle>();

            if (luzPuzzle != null)
            {
                luzPuzzle.RecibirGolpe();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            radioAtaque
        );
    }
}