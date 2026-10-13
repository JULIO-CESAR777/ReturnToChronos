using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseMainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Primera selección")]
    [SerializeField] private Selectable resumeFirstSelected;
    [SerializeField] private Selectable settingsFirstSelected;

    [SerializeField] private PauseMenuTransition pauseTransition;

    [Header("Escenas")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused;
    private bool isInSettings;

    private void Start()
    {
      
        isPaused = false;
        isInSettings = false;
    }

    private void Update()
    {
        // No aceptar inputs mientras hay transición.
        if (SceneTransitionManager.Instance != null &&
            SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        bool pausePressed = false;
        bool backPressed = false;


        // =============================================
        // TECLADO
        // =============================================

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                pausePressed = true;
                backPressed = true;
            }
        }


        // =============================================
        // CONTROL
        // =============================================

        if (Gamepad.current != null)
        {
            // Start / Options
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                pausePressed = true;
            }

            // Xbox B / PlayStation Circle
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                backPressed = true;
            }
        }


        // =============================================
        // NO ESTAMOS EN PAUSA
        // =============================================

        if (!isPaused)
        {
            if (pausePressed)
                OpenPauseMenu();

            return;
        }


        // =============================================
        // ESTAMOS EN SETTINGS
        // =============================================

        if (isInSettings)
        {
            if (backPressed)
            {
                OpenPauseMain();
            }

            return;
        }


        // =============================================
        // ESTAMOS EN PAUSE MAIN
        // =============================================

        if (pausePressed || backPressed)
        {
            ResumeGame();
        }
    }


    // =========================================================
    // ABRIR PAUSA
    // =========================================================

    public void OpenPauseMenu()
    {
        if (isPaused)
            return;

        Time.timeScale = 0;
        
        isPaused = true;
        isInSettings = false;

        // IMPORTANTE:
        // No usamos Time.timeScale = 0.
        // El mundo sigue funcionando.

   

        pauseTransition.Show();

        pauseMainPanel.SetActive(true);
        settingsPanel.SetActive(false);

        Select(resumeFirstSelected);

     
    }


    // =========================================================
    // REANUDAR
    // =========================================================

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        isInSettings = false;

        EventSystem.current?.SetSelectedGameObject(null);
        
        Time.timeScale = 1;
        
        pauseTransition.Hide();
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (!isPaused)
            return;

        isInSettings = true;

        pauseMainPanel.SetActive(false);
        settingsPanel.SetActive(true);

        Select(settingsFirstSelected);
    }


    // =========================================================
    // VOLVER DE SETTINGS
    // =========================================================

    public void OpenPauseMain()
    {
        isInSettings = false;

        settingsPanel.SetActive(false);
        pauseMainPanel.SetActive(true);

        Select(resumeFirstSelected);
    }


    // =========================================================
    // SALIR AL MAIN MENU
    // =========================================================

    public void ExitToMainMenu()
    {
        // Volvemos a habilitar input antes de cambiar escena
        // por seguridad.
 

        isPaused = false;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(
                mainMenuScene
            );
        }
    }


    // =========================================================
    // PLAYER INPUT
    // =========================================================



 

    // =========================================================
    // UI SELECTION
    // =========================================================

    private void Select(Selectable selectable)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);

        if (selectable == null)
            return;

        EventSystem.current.SetSelectedGameObject(
            selectable.gameObject
        );
    }
}