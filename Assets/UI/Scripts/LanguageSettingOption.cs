using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LanguageSettingOption : MonoBehaviour,
    ISelectHandler,
    IDeselectHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private bool isSelected;

    private void Start()
    {
        if (leftButton != null)
            leftButton.onClick.AddListener(Previous);

        if (rightButton != null)
            rightButton.onClick.AddListener(Next);

        Refresh();
    }

    private void Update()
    {
        if (!isSelected)
            return;

        bool left = false;
        bool right = false;

        // TECLADO
        if (Keyboard.current != null)
        {
            left =
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame;

            right =
                Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame;
        }

        // CONTROL
        if (Gamepad.current != null)
        {
            left |=
                Gamepad.current.dpad.left.wasPressedThisFrame ||
                Gamepad.current.leftStick.left.wasPressedThisFrame;

            right |=
                Gamepad.current.dpad.right.wasPressedThisFrame ||
                Gamepad.current.leftStick.right.wasPressedThisFrame;
        }

        if (left)
            Previous();
        else if (right)
            Next();
    }

    public void Previous()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.ChangeLanguage(-1);

        Refresh();
        ReselectOption();
    }

    public void Next()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.ChangeLanguage(1);

        Refresh();
        ReselectOption();
    }

    private void ReselectOption()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    private void Refresh()
    {
        if (SettingsManager.Instance == null || valueText == null)
            return;

        valueText.text =
            SettingsManager.Instance.GetLanguageName();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }

    private void OnDestroy()
    {
        if (leftButton != null)
            leftButton.onClick.RemoveListener(Previous);

        if (rightButton != null)
            rightButton.onClick.RemoveListener(Next);
    }
}