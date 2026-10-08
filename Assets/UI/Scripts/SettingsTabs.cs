using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsTabs : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject stadisticsPanel;

    [Header("Primer elemento")]
    [SerializeField] private Selectable generalFirstSelected;
    [SerializeField] private Selectable controlsFirstSelected;
    [SerializeField] private Selectable stadisticsFirstSelected;

    private int currentTab;

    private void OnEnable()
    {
        OpenGeneral();
    }

    private void Update()
    {
        bool nextTab = false;
        bool previousTab = false;

        // TECLADO

        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                nextTab = true;

            if (Keyboard.current.qKey.wasPressedThisFrame)
                previousTab = true;
        }

        // CONTROL

        if (Gamepad.current != null)
        {
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                nextTab = true;

            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                previousTab = true;
        }

        if (nextTab)
            NextTab();

        if (previousTab)
            PreviousTab();
    }

    public void NextTab()
    {
        currentTab++;

        if (currentTab > 2)
            currentTab = 0;

        RefreshTab();
    }

    public void PreviousTab()
    {
        currentTab--;

        if (currentTab < 0)
            currentTab = 1;

        RefreshTab();
    }

    public void OpenGeneral()
    {
        currentTab = 0;

        generalPanel.SetActive(true);
        controlsPanel.SetActive(false);
        stadisticsPanel.SetActive(false);


        Select(generalFirstSelected);
    }

    public void OpenControls()
    {
        currentTab = 1;

        generalPanel.SetActive(false);
        controlsPanel.SetActive(true);
        stadisticsPanel.SetActive(false);


        Select(controlsFirstSelected);
    }
    
    public void OpenStadistics()
    {
        currentTab = 2;

        generalPanel.SetActive(false);
        controlsPanel.SetActive(false);
        stadisticsPanel.SetActive(true);
        
        Select(stadisticsFirstSelected);
    }

    private void RefreshTab()
    {
        switch (currentTab)
        {
            case 0:
                OpenGeneral();
                break;

            case 1:
                OpenControls();
                break;
            
            case 2:
                OpenStadistics();
                break;
        }
    }

    private void Select(Selectable selectable)
    {
        if (selectable == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}