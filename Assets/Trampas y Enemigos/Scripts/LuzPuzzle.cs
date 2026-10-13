using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class LuzPuzzle : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Light luzObjetivo;
    [SerializeField] private Renderer renderizadorObjetivo;

    // Multiplicador URP
    [SerializeField] private float multiplicadorIntensidadLuz = 300f;

    [SerializeField] private Color colorApagado = new Color(0.12f, 0.12f, 0.14f);
    [SerializeField] private Color colorReposo = new Color(0.25f, 0.55f, 1f);
    [SerializeField] private Color colorPista = Color.white;
    [SerializeField] private Color colorSeleccionado = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color colorAcierto = new Color(0.2f, 1f, 0.35f);
    [SerializeField] private Color colorError = new Color(1f, 0.2f, 0.2f);

    [Header("Interaccion")]
    [SerializeField] private bool usarDistanciaAdicional = false;
    [SerializeField] private float rangoInteraccion = 2.5f;

    [Header("Feedback")]
    [SerializeField] private float duracionFeedbackCorrecto = 1f;

    private Coroutine feedbackSeleccionado;
    private Material materialObjetivo;
    private PuzzleSecuenciaLuces puzzle;
    private int indice;

    private int collidersDentroJugador = 0;
    private Transform jugador;

    public int Indice => indice;

    public void Configurar(PuzzleSecuenciaLuces duenio, int indiceLuz)
    {
        puzzle = duenio;
        indice = indiceLuz;

        if (luzObjetivo == null)
            luzObjetivo = GetComponentInChildren<Light>();

        if (renderizadorObjetivo == null)
            renderizadorObjetivo = GetComponentInChildren<Renderer>();

        if (renderizadorObjetivo != null)
            materialObjetivo = renderizadorObjetivo.material;

        MostrarApagado();
    }

    private void Update()
    {
        if (collidersDentroJugador <= 0 || puzzle == null)
            return;

        if (!puzzle.AceptaEntrada)
            return;

        if (!SePulsoInteractuar())
            return;

        if (usarDistanciaAdicional && jugador != null)
        {
            float distancia = Vector3.Distance(jugador.position, transform.position);
            if (distancia > rangoInteraccion)
                return;
        }

        ActivarLuz();
    }

    public void RecibirGolpe()
    {
        if (puzzle == null || !puzzle.AceptaEntrada)
            return;
        Debug.Log($"Luz {indice} recibió golpe, intentando activar en puzzle.");

        ActivarLuz();
    }

    private void ActivarLuz()
    {
        if (puzzle != null)
            puzzle.IntentarActivar(this);
    }

    private void OnTriggerEnter(Collider otro)
    {
        // Verifica si la raíz del objeto tiene el Tag "Player"
        if (!otro.transform.root.CompareTag("Player"))
            return;

        collidersDentroJugador++;
        jugador = otro.transform.root;
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!otro.transform.root.CompareTag("Player"))
            return;

        collidersDentroJugador--;

        if (collidersDentroJugador <= 0)
        {
            collidersDentroJugador = 0;
            jugador = null;
        }
    }

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
        feedbackSeleccionado = StartCoroutine(FeedbackCorrecto());
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

    private void CancelarFeedback()
    {
        if (feedbackSeleccionado == null)
            return;

        StopCoroutine(feedbackSeleccionado);
        feedbackSeleccionado = null;
    }

    private void AplicarVisual(Color color, float intensidad)
    {
        bool encendida = intensidad > 0.01f;

        if (luzObjetivo != null)
        {
            luzObjetivo.color = color;
            luzObjetivo.intensity = intensidad * multiplicadorIntensidadLuz;
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
                    materialObjetivo.SetColor("_EmissionColor", color * intensidad);
                }
                else
                {
                    materialObjetivo.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    private IEnumerator FeedbackCorrecto()
    {
        AplicarVisual(colorSeleccionado, 4f);
        yield return new WaitForSeconds(duracionFeedbackCorrecto);
        feedbackSeleccionado = null;
        AplicarVisual(colorReposo, 1.2f);
    }

    private static bool SePulsoInteractuar()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado.eKey.wasPressedThisFrame)
            return true;

        Gamepad mando = Gamepad.current;
        return mando != null && mando.buttonNorth.wasPressedThisFrame;
    }

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