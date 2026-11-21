using System;
using System.Collections;
using UnityEngine;

public class PauseMenuTransition : MonoBehaviour
{
    [Header("Objetos")]
    [SerializeField] private GameObject pauseMenuRoot;

    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private CanvasGroup panelGroup;

    [SerializeField] private RectTransform pausePanel;

    [Header("Animación")]
    [SerializeField] private float duration = 0.3f;

    [Tooltip("Cuántos píxeles entra desde la izquierda")]
    [SerializeField] private float horizontalOffset = 80f;

    private Vector2 normalPosition;

    private Coroutine currentTransition;

    private void Awake()
    {
        normalPosition = pausePanel.anchoredPosition;

        pauseMenuRoot.SetActive(false);
    }

    // =====================================================
    // ABRIR
    // =====================================================

    public void Show()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(
            ShowCoroutine()
        );
    }

    private IEnumerator ShowCoroutine()
    {
        pauseMenuRoot.SetActive(true);

        backgroundGroup.alpha = 0f;
        panelGroup.alpha = 0f;

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        Vector2 startPosition =
            normalPosition + Vector2.left * horizontalOffset;

        pausePanel.anchoredPosition = startPosition;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                time / duration
            );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            // Oscurecer fondo
            backgroundGroup.alpha = smoothT;

            // Mostrar menú
            panelGroup.alpha = smoothT;

            // Mover suavemente hacia su posición
            pausePanel.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    normalPosition,
                    smoothT
                );

            yield return null;
        }

        backgroundGroup.alpha = 1f;
        panelGroup.alpha = 1f;

        pausePanel.anchoredPosition =
            normalPosition;

        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;

        currentTransition = null;
    }


    // =====================================================
    // CERRAR
    // =====================================================

    public void Hide(Action onFinished = null)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(
            HideCoroutine(onFinished)
        );
    }

    private IEnumerator HideCoroutine(
        Action onFinished)
    {
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        Vector2 endPosition =
            normalPosition + Vector2.left * horizontalOffset;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                time / duration
            );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            backgroundGroup.alpha =
                Mathf.Lerp(1f, 0f, smoothT);

            panelGroup.alpha =
                Mathf.Lerp(1f, 0f, smoothT);

            pausePanel.anchoredPosition =
                Vector2.Lerp(
                    normalPosition,
                    endPosition,
                    smoothT
                );

            yield return null;
        }

        backgroundGroup.alpha = 0f;
        panelGroup.alpha = 0f;

        pausePanel.anchoredPosition =
            normalPosition;

        pauseMenuRoot.SetActive(false);

        currentTransition = null;

        onFinished?.Invoke();
    }
}