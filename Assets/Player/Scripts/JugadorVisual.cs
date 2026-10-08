using UnityEngine;

public class JugadorVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControladorJugador jugador;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform camara;

    [Header("Sprite")]
    [Tooltip("Activa esto si tu sprite original mira hacia la derecha.")]
    [SerializeField] private bool spriteOriginalMiraDerecha = true;
    [SerializeField] private AtaqueJugador ataqueJugador;
    private void Awake()
    {
        if (jugador == null)
        {
            jugador =
                GetComponentInParent<ControladorJugador>();
        }

        if (ataqueJugador == null)
        {
            ataqueJugador = GetComponentInParent<AtaqueJugador>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (camara == null &&
            Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    private void Update()
    {
        ActualizarAnimacion();

        ActualizarDireccionVisual();
    }

    private void LateUpdate()
    {
        MirarHaciaCamara();
    }

    // =========================================================
    // BILLBOARD
    // =========================================================

    private void MirarHaciaCamara()
    {
        if (camara == null)
            return;

        transform.rotation =
            Quaternion.LookRotation(
                -camara.forward,
                camara.up
            );
    }

    // =========================================================
    // ANIMACIÓN
    // =========================================================

    private void ActualizarAnimacion()
    {
        if (animator == null ||
            jugador == null)
        {
            return;
        }

        animator.SetBool(
            "Moviendose",
            jugador.EstaMoviendose
        );
    }

    // =========================================================
    // IZQUIERDA / DERECHA
    // =========================================================

    private void ActualizarDireccionVisual()
    {
        if (jugador == null ||
            spriteRenderer == null ||
            camara == null)
        {
            return;
        }

        Vector3 direccion =
            jugador.DireccionMirada;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        float lado =
            Vector3.Dot(
                direccion.normalized,
                camara.right
            );

        if (Mathf.Abs(lado) < 0.05f)
            return;

        bool miraIzquierda =
            lado > 0f;

        if (spriteOriginalMiraDerecha)
        {
            spriteRenderer.flipX =
                miraIzquierda;
        }
        else
        {
            spriteRenderer.flipX =
                !miraIzquierda;
        }
    }

    // =========================================================
    // ATAQUE
    // =========================================================

    public void ReproducirAtaque()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Atacar");
    }
    
    public void EventoAplicarGolpe()
    {
        if (ataqueJugador != null)
        {
            ataqueJugador.AplicarGolpe();
        }
    }
}