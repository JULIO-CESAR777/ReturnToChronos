using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ResolutionSettingOption : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

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
        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        // TECLADO
        if (Keyboard.current != null)
        {
            if (
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame
            )
            {
                Previous();
                return;
            }

            if (
                Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame
            )
            {
                Next();
                return;
            }
        }

        // CONTROL
        if (Gamepad.current != null)
        {
            if (
                Gamepad.current.dpad.left.wasPressedThisFrame ||
                Gamepad.current.leftStick.left.wasPressedThisFrame
            )
            {
                Previous();
                return;
            }

            if (
                Gamepad.current.dpad.right.wasPressedThisFrame ||
                Gamepad.current.leftStick.right.wasPressedThisFrame
            )
            {
                Next();
            }
        }
    }

    public void Previous()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.ChangeDisplayPreset(-1);

        Refresh();
        Reselect();
    }

    public void Next()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.ChangeDisplayPreset(1);

        Refresh();
        Reselect();
    }

    private void Refresh()
    {
        if (SettingsManager.Instance == null)
            return;

        valueText.text =
            SettingsManager.Instance.GetDisplayPresetName();
    }

    private void Reselect()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }
}