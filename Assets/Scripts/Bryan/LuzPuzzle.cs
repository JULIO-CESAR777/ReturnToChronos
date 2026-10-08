using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
/// <summary>
/// Luz interactuable del puzzle.
/// Se puede activar interactuando cerca
/// o recibiendo un golpe del jugador.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LuzPuzzle : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Light luzObjetivo;
    [SerializeField] private Renderer renderizadorObjetivo;

    [SerializeField] private Color colorApagado =
        new Color(0.12f, 0.12f, 0.14f);

    [SerializeField] private Color colorReposo =
        new Color(0.25f, 0.55f, 1f);

    [SerializeField] private Color colorPista =
        Color.white;

    [SerializeField] private Color colorSeleccionado =
        new Color(1f, 0.85f, 0.2f);

    [SerializeField] private Color colorAcierto =
        new Color(0.2f, 1f, 0.35f);

    [SerializeField] private Color colorError =
        new Color(1f, 0.2f, 0.2f);


    [Header("Interaccion")]
    [SerializeField] private float rangoInteraccion = 2.2f;
    
    [Header("Feedback")]
    [SerializeField] private float duracionFeedbackCorrecto = 1f;

    private Coroutine feedbackSeleccionado;


    private PuzzleSecuenciaLuces puzzle;

    private int indice;

    private bool jugadorDentro;

    private Transform jugador;


    public int Indice => indice;


    // =========================================================
    // CONFIGURACION
    // =========================================================

    public void Configurar(
        PuzzleSecuenciaLuces duenio,
        int indiceLuz
    )
    {
        puzzle = duenio;

        indice = indiceLuz;


        if (luzObjetivo == null)
        {
            luzObjetivo =
                GetComponentInChildren<Light>();
        }


        if (renderizadorObjetivo == null)
        {
            renderizadorObjetivo =
                GetComponentInChildren<Renderer>();
        }


        AplicarVisual(
            colorApagado,
            0f
        );
    }


    // =========================================================
    // INTERACCION NORMAL
    // =========================================================

    private void Update()
    {
        if (!jugadorDentro)
            return;


        if (puzzle == null)
            return;


        if (!puzzle.AceptaEntrada)
            return;


        if (!SePulsoInteractuar())
            return;


        if (jugador != null)
        {
            float distancia =
                Vector3.Distance(
                    jugador.position,
                    transform.position
                );


            if (distancia > rangoInteraccion)
                return;
        }


        ActivarLuz();
    }


    // =========================================================
    // RECIBIR GOLPE
    // =========================================================

    public void RecibirGolpe()
    {
        if (puzzle == null)
            return;


        if (!puzzle.AceptaEntrada)
            return;


        ActivarLuz();
    }


    // =========================================================
    // ACTIVAR
    // =========================================================

    private void ActivarLuz()
    {
        puzzle.IntentarActivar(this);
    }


    // =========================================================
    // TRIGGER JUGADOR
    // =========================================================

    private void OnTriggerEnter(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;


        jugadorDentro = true;

        jugador =
            otro.transform;
    }


    private void OnTriggerExit(Collider otro)
    {
        if (!otro.CompareTag("Player"))
            return;


        jugadorDentro = false;


        if (jugador == otro.transform)
        {
            jugador = null;
        }
    }


    // =========================================================
    // VISUALES
    // =========================================================

    public void MostrarPista()
    {
        AplicarVisual(
            colorPista,
            3.5f
        );
    }


    public void MostrarReposo()
    {
        AplicarVisual(
            colorReposo,
            1.2f
        );
    }


    public void MostrarSeleccionado()
    {
        if (feedbackSeleccionado != null)
        {
            StopCoroutine(feedbackSeleccionado);
        }

        feedbackSeleccionado =
            StartCoroutine(FeedbackCorrecto());
    }
    

    public void MostrarAcierto()
    {
        AplicarVisual(
            colorAcierto,
            5f
        );
    }


    public void MostrarError()
    {
        AplicarVisual(
            colorError,
            4f
        );
    }


    public void MostrarApagado()
    {
        AplicarVisual(
            colorApagado,
            0f
        );
    }


    private void AplicarVisual(
        Color color,
        float intensidad
    )
    {
        if (luzObjetivo != null)
        {
            luzObjetivo.color =
                color;

            luzObjetivo.intensity =
                intensidad;

            luzObjetivo.enabled =
                intensidad > 0.01f;
        }


        if (renderizadorObjetivo != null)
        {
            Material material =
                renderizadorObjetivo.material;


            material.color =
                color;


            if (
                material.HasProperty(
                    "_EmissionColor"
                )
            )
            {
                material.EnableKeyword(
                    "_EMISSION"
                );


                material.SetColor(
                    "_EmissionColor",
                    color *
                    Mathf.Max(
                        intensidad,
                        0.2f
                    )
                );
            }
        }
    }


    private IEnumerator FeedbackCorrecto()
    {
        // Se prende indicando que fue correcta
        AplicarVisual(
            colorSeleccionado,
            4f
        );

        // Dura 1 segundo
        yield return new WaitForSeconds(
            duracionFeedbackCorrecto
        );

        // Vuelve al estado normal
        AplicarVisual(
            colorReposo,
            1.2f
        );

        feedbackSeleccionado = null;
    }
    // =========================================================
    // INPUT INTERACCION
    // =========================================================

    private static bool SePulsoInteractuar()
    {
        Keyboard teclado =
            Keyboard.current;


        if (
            teclado != null &&
            teclado.eKey.wasPressedThisFrame
        )
        {
            return true;
        }


        Gamepad mando =
            Gamepad.current;


        return
            mando != null &&
            mando.buttonNorth.wasPressedThisFrame;
    }
}