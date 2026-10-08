using UnityEngine;
using UnityEngine.UI;

public class LocalizedImage : MonoBehaviour
{
    [Header("Imagen que cambiará")]
    [SerializeField] private Image conversionImage;

    [Header("Imágenes por idioma")]
    [SerializeField] private Sprite spanishSprite;
    [SerializeField] private Sprite englishSprite;

    private void Awake()
    {
        if (conversionImage == null)
            conversionImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        SettingsManager.LanguageChanged += OnLanguageChanged;

        Refresh();
    }

    private void OnDisable()
    {
        SettingsManager.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(
        SettingsManager.GameLanguage language
    )
    {
        Refresh();
    }

    private void Refresh()
    {
        if (SettingsManager.Instance == null)
            return;

        if (conversionImage == null)
            return;

        switch (SettingsManager.Instance.CurrentLanguage)
        {
            case SettingsManager.GameLanguage.English:
                conversionImage.sprite = englishSprite;
                break;

            case SettingsManager.GameLanguage.Spanish:
            default:
                conversionImage.sprite = spanishSprite;
                break;
        }
    }
}