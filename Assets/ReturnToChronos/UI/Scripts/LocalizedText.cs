using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;

    [Header("Traducciones")]
    [TextArea]
    [SerializeField] private string spanish;

    [TextArea]
    [SerializeField] private string english;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        SettingsManager.LanguageChanged += OnLanguageChanged;
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        SettingsManager.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(SettingsManager.GameLanguage language)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (SettingsManager.Instance == null || targetText == null)
            return;

        switch (SettingsManager.Instance.CurrentLanguage)
        {
            case SettingsManager.GameLanguage.English:
                targetText.text = english;
                break;

            default:
                targetText.text = spanish;
                break;
        }
    }
}