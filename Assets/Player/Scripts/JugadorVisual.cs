using UnityEngine;

public class JugadorVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControladorJugador jugador;
    [SerializeField] private AtaqueJugador ataqueJugador;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform camara;


    private void Awake()
    {
        if (jugador == null)
        {
            jugador =
                GetComponentInParent<ControladorJugador>();
        }

        if (ataqueJugador == null)
        {
            ataqueJugador =
                GetComponentInParent<AtaqueJugador>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (camara == null &&
            Camera.main != null)
        {
            camara =
                Camera.main.transform;
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
    // MOVIMIENTO
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
    // DIRECCION DE LA ANIMACION
    // =========================================================

    private void ActualizarDireccionVisual()
    {
        if (jugador == null ||
            animator == null ||
            camara == null)
        {
            return;
        }


        // =====================================================
        // DIRECCION QUE DEBE MIRAR EL PERSONAJE
        // =====================================================

        Vector3 direccion;

        // Si está apuntando con stick derecho / ratón,
        // usamos la dirección de apuntado.
        if (jugador.EstaApuntando)
        {
            direccion =
                jugador.DireccionMirada;
        }
        else
        {
            // Si no está apuntando,
            // usamos hacia dónde está caminando.
            direccion =
                jugador.DireccionMovimiento;
        }


        if (direccion.sqrMagnitude < 0.01f)
            return;


        direccion.Normalize();


        // =====================================================
        // EJES DE LA CAMARA
        // =====================================================

        Vector3 derechaCamara =
            camara.right;

        Vector3 adelanteCamara =
            camara.forward;


        derechaCamara.y = 0f;
        adelanteCamara.y = 0f;


        derechaCamara.Normalize();
        adelanteCamara.Normalize();


        // =====================================================
        // CONVERTIMOS DIRECCION DEL MUNDO
        // A DIRECCION VISUAL DE PANTALLA
        // =====================================================

        float dirX =
            Vector3.Dot(
                direccion,
                derechaCamara
            );

        float dirY =
            Vector3.Dot(
                direccion,
                adelanteCamara
            );


        animator.SetFloat(
            "DirX",
            dirX
        );

        animator.SetFloat(
            "DirY",
            dirY
        );
    }


    // =========================================================
    // ATAQUE
    // =========================================================

    public void ReproducirAtaque()
    {
        if (animator == null)
            return;

        animator.SetTrigger(
            "Atacar"
        );
    }


    // =========================================================
    // ANIMATION EVENT DEL GOLPE
    // =========================================================

    public void EventoAplicarGolpe()
    {
        Debug.Log(
            $"[ANIMACIÓN] EventoAplicarGolpe ejecutado. " +
            $"Frame: {Time.frameCount}, " +
            $"Objeto: {gameObject.name}, " +
            $"Instancia: {GetInstanceID()}"
        );

        if (ataqueJugador != null)
        {
            ataqueJugador.AplicarGolpe();
        }
        else
        {
            Debug.LogWarning(
                "[ANIMACIÓN] No se encontró AtaqueJugador."
            );
        }
    }