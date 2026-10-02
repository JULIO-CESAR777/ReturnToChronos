using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Luz interactuable del puzzle. El jugador pulsa E cerca para activarla.
/// Necesita un Collider con Is Trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LuzPuzzle : MonoBehaviour
{
    [SerializeField] Light luzObjetivo;
    [SerializeField] Renderer renderizadorObjetivo;
    [SerializeField] Color colorApagado = new Color(0.12f, 0.12f, 0.14f);
    [SerializeField] Color colorReposo = new Color(0.25f, 0.55f, 1f);
    [SerializeField] Color colorPista = Color.white;
    [SerializeField] Color colorSeleccionado = new Color(1f, 0.85f, 0.2f);
    [SerializeField] Color colorAcierto = new Color(0.2f, 1f, 0.35f);
    [SerializeField] Color colorError = new Color(1f, 0.2f, 0.2f);
    [SerializeField] float rangoInteraccion = 2.2f;

    PuzzleSecuenciaLuces puzzle;
    int indice;
    bool jugadorDentro;
    Transform jugador;

    public int Indice => indice;

    public void Configurar(PuzzleSecuenciaLuces duenio, int indiceLuz)
    {
        puzzle = duenio;
        indice = indiceLuz;
        if (luzObjetivo == null)
            luzObjetivo = GetComponentInChildren<Light>();
        if (renderizadorObjetivo == null)
            renderizadorObjetivo = GetComponentInChildren<Renderer>();
        AplicarVisual(colorApagado, 0f);
    }

    void Update()
    {
        if (!jugadorDentro || puzzle == null || !puzzle.AceptaEntrada)
            return;

        if (!SePulsoInteractuar())
            return;

        if (jugador != null && Vector3.Distance(jugador.position, transform.position) > rangoInteraccion)
            return;

        puzzle.IntentarActivar(this);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        jugadorDentro = true;
        jugador = otro.transform;
    }

    void OnTriggerExit(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        jugadorDentro = false;
        if (jugador == otro.transform)
            jugador = null;
    }

    public void MostrarPista()
    {
        AplicarVisual(colorPista, 3.5f);
    }

    public void MostrarReposo()
    {
        AplicarVisual(colorReposo, 1.2f);
    }

    public void MostrarSeleccionado()
    {
        AplicarVisual(colorSeleccionado, 4f);
    }

    public void MostrarAcierto()
    {
        AplicarVisual(colorAcierto, 5f);
    }

    public void MostrarError()
    {
        AplicarVisual(colorError, 4f);
    }

    public void MostrarApagado()
    {
        AplicarVisual(colorApagado, 0f);
    }

    void AplicarVisual(Color color, float intensidad)
    {
        if (luzObjetivo != null)
        {
            luzObjetivo.color = color;
            luzObjetivo.intensity = intensidad;
            luzObjetivo.enabled = intensidad > 0.01f;
        }

        if (renderizadorObjetivo != null)
        {
            Material material = renderizadorObjetivo.material;
            material.color = color;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(intensidad, 0.2f));
            }
        }
    }

    static bool SePulsoInteractuar()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado.eKey.wasPressedThisFrame)
            return true;

        Gamepad mando = Gamepad.current;
        return mando != null && mando.buttonNorth.wasPressedThisFrame;
    }
}
