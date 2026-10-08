using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public enum MenuType
    {
        Main,
        Slots,
        Settings,
        Credits
    }

    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject slotsMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;

    [Header("Primer elemento de cada menú")]
    [SerializeField] private Selectable mainFirstSelected;
    [SerializeField] private Selectable slotsFirstSelected;
    [SerializeField] private Selectable settingsFirstSelected;
    [SerializeField] private Selectable creditsFirstSelected;

    private MenuType currentMenu;

    private void Awake()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        SceneTransitionManager.Instance.TransitionMenu(
            () => ApplyMainMenu()
        );
    }

    private void ApplyMainMenu()
    {
        DisableAllMenus();

        if (mainMenu != null)
            mainMenu.SetActive(true);

        currentMenu = MenuType.Main;

        Select(mainFirstSelected);
    }

    public void OpenSlotsMenu()
    {
        SceneTransitionManager.Instance.LoadScene("Nivel1_Dungeon");
    }

    private void ApplySlotsMenu()
    {
        DisableAllMenus();

        if (slotsMenu != null)
            slotsMenu.SetActive(true);

        currentMenu = MenuType.Slots;

        Select(slotsFirstSelected);
    }

    public void OpenSettingsMenu()
    {
        SceneTransitionManager.Instance.TransitionMenu(
            () => ApplySettingsMenu()
        );
    }

    private void ApplySettingsMenu()
    {
        DisableAllMenus();

        settingsMenu.SetActive(true);

        currentMenu = MenuType.Settings;

        Select(settingsFirstSelected);
    }

   

    public void OpenCreditsMenu()
    {
        SceneTransitionManager.Instance.TransitionMenu(
            () => ApplyCreditsMenu()
        );
    }

    private void ApplyCreditsMenu()
    {
        DisableAllMenus();

        if (creditsMenu != null)
            creditsMenu.SetActive(true);

        currentMenu = MenuType.Credits;

        Select(creditsFirstSelected);
    }
    
    public void Back()
    {
        switch (currentMenu)
        {
            case MenuType.Slots:
            case MenuType.Settings:
            case MenuType.Credits:
                OpenMainMenu();
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void DisableAllMenus()
    {
        mainMenu.SetActive(false);
        slotsMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    private void Select(Selectable selectable)
    {
        if (EventSystem.current == null)
            return;

     
        EventSystem.current.SetSelectedGameObject(null);

        if (selectable == null)
            return;

        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}