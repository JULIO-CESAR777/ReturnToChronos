using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform transitionSymbol;

    [Header("Cambio de escena")]
    [SerializeField] private float sceneFadeIn = 0.6f;
    [SerializeField] private float sceneFadeOut = 0.6f;

    [Header("Cambio de menú")]
    [SerializeField] private float menuFadeIn = 0.25f;
    [SerializeField] private float menuFadeOut = 0.25f;

    [Header("Símbolo")]
    [SerializeField] private float rotationSpeed = 180f;

    [SerializeField] private Vector3 startScale =
        new Vector3(0.3f, 0.3f, 1f);

    [SerializeField] private Vector3 fullScale =
        new Vector3(2.5f, 2.5f, 1f);

    [Header("Carga")]
    [Tooltip("Tiempo mínimo que la pantalla permanece cubierta al cargar una escena.")]
    [SerializeField] private float minimumLoadingTime = 0.5f;

    private bool isTransitioning;
    private bool rotateSymbol;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (transitionSymbol != null)
            transitionSymbol.localScale = startScale;
    }

    private void Update()
    {
        // El símbolo gira TODO el tiempo que dure la transición,
        // incluso mientras Unity está cargando la escena.
        if (rotateSymbol && transitionSymbol != null)
        {
            transitionSymbol.Rotate(
                0f,
                0f,
                rotationSpeed * Time.unscaledDeltaTime
            );
        }
    }

    // =========================================================
    // CAMBIAR ESCENA
    // =========================================================

    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(SceneTransition(sceneName));
    }

    private IEnumerator SceneTransition(string sceneName)
    {
        isTransitioning = true;
        rotateSymbol = true;

        canvasGroup.blocksRaycasts = true;

        // =====================================================
        // 1. CUBRIR LA ESCENA ACTUAL
        // =====================================================

        yield return Fade(
            0f,
            1f,
            sceneFadeIn,
            startScale,
            fullScale
        );

        // A partir de aquí la pantalla queda completamente tapada.
        canvasGroup.alpha = 1f;

        if (transitionSymbol != null)
            transitionSymbol.localScale = fullScale;


        // =====================================================
        // 2. EMPEZAR A CARGAR
        // =====================================================

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        // Impide que Unity active la escena inmediatamente
        // cuando termina de cargarla.
        operation.allowSceneActivation = false;

        float loadingTimer = 0f;


        // =====================================================
        // 3. ESPERAR HASTA QUE LA ESCENA ESTÉ LISTA
        // =====================================================

        // Unity llega aproximadamente a 0.9 cuando ya cargó
        // prácticamente toda la escena y solo falta activarla.
        while (operation.progress < 0.9f)
        {
            loadingTimer += Time.unscaledDeltaTime;

            yield return null;
        }


        // Opcional:
        // evita un flash demasiado rápido si la escena cargó
        // prácticamente instantáneamente.
        while (loadingTimer < minimumLoadingTime)
        {
            loadingTimer += Time.unscaledDeltaTime;

            yield return null;
        }


        // =====================================================
        // 4. ACTIVAR LA NUEVA ESCENA
        // =====================================================

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }


        // Dejamos que la nueva escena ejecute al menos un frame.
        yield return null;


        // =====================================================
        // 5. DESCUBRIR LA NUEVA ESCENA
        // =====================================================

        yield return Fade(
            1f,
            0f,
            sceneFadeOut,
            fullScale,
            startScale
        );


        // =====================================================
        // TERMINAR
        // =====================================================

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        rotateSymbol = false;
        isTransitioning = false;
    }


    // =========================================================
    // CAMBIAR PANEL / MENÚ
    // =========================================================

    public void TransitionMenu(Action changeMenu)
    {
        if (isTransitioning)
            return;

        StartCoroutine(
            MenuTransition(changeMenu)
        );
    }

    private IEnumerator MenuTransition(Action changeMenu)
    {
        isTransitioning = true;
        rotateSymbol = true;

        canvasGroup.blocksRaycasts = true;


        // Tapa el menú actual.
        yield return Fade(
            0f,
            1f,
            menuFadeIn,
            startScale,
            fullScale
        );


        // Cambia realmente el panel.
        changeMenu?.Invoke();

        yield return null;


        // Muestra el nuevo menú.
        yield return Fade(
            1f,
            0f,
            menuFadeOut,
            fullScale,
            startScale
        );


        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        rotateSymbol = false;
        isTransitioning = false;
    }


    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator Fade(
        float startAlpha,
        float endAlpha,
        float duration,
        Vector3 fromScale,
        Vector3 toScale)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;

            if (transitionSymbol != null)
                transitionSymbol.localScale = toScale;

            yield break;
        }


        float time = 0f;

        canvasGroup.alpha = startAlpha;

        if (transitionSymbol != null)
            transitionSymbol.localScale = fromScale;


        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(time / duration);

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);


            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    smoothT
                );


            if (transitionSymbol != null)
            {
                transitionSymbol.localScale =
                    Vector3.Lerp(
                        fromScale,
                        toScale,
                        smoothT
                    );
            }


            yield return null;
        }


        canvasGroup.alpha = endAlpha;

        if (transitionSymbol != null)
            transitionSymbol.localScale = toScale;
    }
}