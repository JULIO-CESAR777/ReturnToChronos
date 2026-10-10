
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
// Se puede activar interactuando cerca
[RequireComponent(typeof(Collider))]
public class LuzPuzzle : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Light luzObjetivo;
    [SerializeField] private Renderer renderizadorObjetivo;

    [SerializeField]
    private Color colorApagado =
        new Color(0.12f, 0.12f, 0.14f);

    [SerializeField]
    private Color colorReposo =
        new Color(0.25f, 0.55f, 1f);

    [SerializeField] private Color colorPista = Color.white;

    [SerializeField]
    private Color colorSeleccionado =
        new Color(1f, 0.85f, 0.2f);

    [SerializeField]
    private Color colorAcierto =
        new Color(0.2f, 1f, 0.35f);

    [SerializeField]
    private Color colorError =
        new Color(1f, 0.2f, 0.2f);

    [Header("Interaccion")]
    [SerializeField] private float rangoInteraccion = 2.2f;

    [Header("Feedback")]
    [SerializeField] private float duracionFeedbackCorrecto = 1f;

    private Coroutine feedbackSeleccionado;

    // Material almacenado para evitar obtenerlo en cada cambio visual.
    private Material materialObjetivo;

    private PuzzleSecuenciaLuces puzzle;
    private int indice;
    private bool jugadorDentro;
    private Transform jugador;

    public int Indice => indice;

    // CONFIGURACION
    public void Configurar(
        PuzzleSecuenciaLuces duenio,
        int indiceLuz)
    {
        puzzle = duenio;
        indice = indiceLuz;

        if (luzObjetivo == null)
            luzObjetivo = GetComponentInChildren<Light>();

        if (renderizadorObjetivo == null)
            renderizadorObjetivo = GetComponentInChildren<Renderer>();

        // Se obtiene una sola vez el material de esta luz.
        if (renderizadorObjetivo != null)
            materialObjetivo = renderizadorObjetivo.material;

        MostrarApagado();
    }

    // INTERACCION NORMAL
    private void Update()
    {
        if (!jugadorDentro || puzzle == null)
            return;

        if (!puzzle.AceptaEntrada)
            return;

        if (!SePulsoInteractuar())
            return;

        if (jugador != null)
        {
            float distancia = Vector3.Distance(
                jugador.position,
                transform.position);

            if (distancia > rangoInteraccion)
                return;
        }

        ActivarLuz();
    }
    // RECIBIR GOLPE

    public void RecibirGolpe()
    {
        if (puzzle == null || !puzzle.AceptaEntrada)
            return;

        ActivarLuz();
    }

    private void ActivarLuz()
    {
        if (puzzle != null)
            puzzle.IntentarActivar(this);
    }
    // TRIGGER JUGADOR

    private void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        jugadorDentro = true;
        jugador = otro.transform;
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        // Evita perder la referencia si hay varios colliders
        // del jugador entrando en el mismo trigger.
        if (jugador == otro.transform)
        {
            jugadorDentro = false;
            jugador = null;
        }
    }
    // VISUALES
    public void MostrarPista()
    {
        CancelarFeedback();
        AplicarVisual(colorPista, 3.5f);
    }

    public void MostrarReposo()
    {
        CancelarFeedback();
        AplicarVisual(colorReposo, 1.2f);
    }

    public void MostrarSeleccionado()
    {
        CancelarFeedback();

        feedbackSeleccionado =
            StartCoroutine(FeedbackCorrecto());
    }

    public void MostrarAcierto()
    {
        CancelarFeedback();
        AplicarVisual(colorAcierto, 5f);
    }

    public void MostrarError()
    {
        CancelarFeedback();
        AplicarVisual(colorError, 4f);
    }

    public void MostrarApagado()
    {
        CancelarFeedback();
        AplicarVisual(colorApagado, 0f);
    }
    // CONTROL DEL FEEDBACK

    private void CancelarFeedback()
    {
        if (feedbackSeleccionado == null)
            return;

        StopCoroutine(feedbackSeleccionado);
        feedbackSeleccionado = null;
    }

    // APLICAR VISUALES
    private void AplicarVisual(Color color, float intensidad)
    {
        bool encendida = intensidad > 0.01f;

        if (luzObjetivo != null)
        {
            luzObjetivo.color = color;
            luzObjetivo.intensity = intensidad;
            luzObjetivo.enabled = encendida;
        }

        if (materialObjetivo != null)
        {
            materialObjetivo.color = color;

            if (materialObjetivo.HasProperty("_EmissionColor"))
            {
                if (encendida)
                {
                    materialObjetivo.EnableKeyword("_EMISSION");

                    materialObjetivo.SetColor(
                        "_EmissionColor",
                        color * intensidad);
                }
                else
                {
                    // Elimina el brillo residual del material.
                    materialObjetivo.SetColor(
                        "_EmissionColor",
                        Color.black);
                }
            }
        }
    }
    // FEEDBACK DE SELECCION
    private IEnumerator FeedbackCorrecto()
    {
        AplicarVisual(colorSeleccionado, 4f);

        yield return new WaitForSeconds(
            duracionFeedbackCorrecto);

        // La corrutina solo puede llegar aquí si no fue cancelada
        // por otro cambio de estado de la luz.
        feedbackSeleccionado = null;

        AplicarVisual(colorReposo, 1.2f);
    }
    // INPUT INTERACCION
    private static bool SePulsoInteractuar()
    {
        Keyboard teclado = Keyboard.current;

        if (teclado != null &&
            teclado.eKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad mando = Gamepad.current;

        return mando != null &&
               mando.buttonNorth.wasPressedThisFrame;
    }
    // LIMPIEZA
    private void OnDestroy()
    {
        CancelarFeedback();

        if (materialObjetivo != null)
        {
            Destroy(materialObjetivo);
            materialObjetivo = null;
        }
    }
}
