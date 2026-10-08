using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLanguage();

        CreateDisplayPresets();
        LoadDisplayPreset();
    }

    // =========================================================
    // IDIOMA
    // =========================================================

    public enum GameLanguage
    {
        Spanish,
        English
    }

    public GameLanguage CurrentLanguage { get; private set; }

    public static event Action<GameLanguage> LanguageChanged;

    private const string LANGUAGE_KEY = "Language";

    private void LoadLanguage()
    {
        int savedLanguage = PlayerPrefs.GetInt(
            LANGUAGE_KEY,
            (int)GameLanguage.Spanish
        );

        if (!Enum.IsDefined(typeof(GameLanguage), savedLanguage))
            savedLanguage = (int)GameLanguage.Spanish;

        CurrentLanguage = (GameLanguage)savedLanguage;
    }

    public void ChangeLanguage(int direction)
    {
        int count = Enum.GetValues(typeof(GameLanguage)).Length;

        int newLanguage = (int)CurrentLanguage + direction;

        if (newLanguage >= count)
            newLanguage = 0;

        if (newLanguage < 0)
            newLanguage = count - 1;

        SetLanguage((GameLanguage)newLanguage);
    }

    public void SetLanguage(GameLanguage language)
    {
        CurrentLanguage = language;

        PlayerPrefs.SetInt(
            LANGUAGE_KEY,
            (int)CurrentLanguage
        );

        PlayerPrefs.Save();

        LanguageChanged?.Invoke(CurrentLanguage);
    }

    public string GetLanguageName()
    {
        switch (CurrentLanguage)
        {
            case GameLanguage.English:
                return "ENGLISH";

            case GameLanguage.Spanish:
            default:
                return "ESPAÑOL";
        }
    }


    // =========================================================
    // PANTALLA / RESOLUCIÓN
    // =========================================================

    public class DisplayPreset
    {
        public int width;
        public int height;

        public FullScreenMode mode;

        public string displayName;

        public DisplayPreset(
            int width,
            int height,
            FullScreenMode mode,
            string displayName)
        {
            this.width = width;
            this.height = height;
            this.mode = mode;
            this.displayName = displayName;
        }
    }

    private List<DisplayPreset> displayPresets =
        new List<DisplayPreset>();

    private int currentDisplayPreset;


    // PlayerPrefs

    private const string DISPLAY_WIDTH_KEY =
        "DisplayWidth";

    private const string DISPLAY_HEIGHT_KEY =
        "DisplayHeight";

    private const string DISPLAY_MODE_KEY =
        "DisplayMode";


    // =========================================================
    // CREAR OPCIONES DE PANTALLA
    // =========================================================

    private void CreateDisplayPresets()
{
    displayPresets.Clear();

    int nativeWidth = Screen.currentResolution.width;
    int nativeHeight = Screen.currentResolution.height;

    float nativeAspect = (float)nativeWidth / nativeHeight;


    // =====================================================
    // 1. PANTALLA COMPLETA
    // Siempre resolución nativa.
    // =====================================================

    AddPreset(
        nativeWidth,
        nativeHeight,
        FullScreenMode.FullScreenWindow,
        $"{nativeWidth} x {nativeHeight} (PANTALLA COMPLETA)"
    );


    // =====================================================
    // 2. RESOLUCIONES QUE WINDOWS / MONITOR SOPORTA
    // Las agregamos únicamente en modo VENTANA.
    // =====================================================

    List<Vector2Int> validResolutions = new List<Vector2Int>();

    foreach (Resolution resolution in Screen.resolutions)
    {
        int width = resolution.width;
        int height = resolution.height;

        // Evitamos resoluciones demasiado pequeñas.
        if (width < 1024 || height < 576)
            continue;

        // Evitamos resoluciones mayores a la pantalla.
        if (width > nativeWidth || height > nativeHeight)
            continue;


        float aspect =
            (float)width / height;

        // Solo dejamos resoluciones con una proporción
        // parecida a la pantalla actual.
        if (Mathf.Abs(aspect - nativeAspect) > 0.03f)
            continue;


        Vector2Int newResolution =
            new Vector2Int(width, height);

        if (!validResolutions.Contains(newResolution))
            validResolutions.Add(newResolution);
    }


    // =====================================================
    // 3. Algunas resoluciones comunes extra
    // =====================================================

    AddCommonResolution(
        validResolutions,
        3840,
        2160,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        2560,
        1440,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        2048,
        1152,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1920,
        1080,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1760,
        990,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1600,
        900,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1536,
        864,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1366,
        768,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1280,
        720,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1152,
        648,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );

    AddCommonResolution(
        validResolutions,
        1024,
        576,
        nativeWidth,
        nativeHeight,
        nativeAspect
    );


    // =====================================================
    // 4. Ordenarlas de mayor a menor
    // =====================================================

    validResolutions.Sort(
        (a, b) =>
        {
            int areaA = a.x * a.y;
            int areaB = b.x * b.y;

            return areaB.CompareTo(areaA);
        }
    );


    // =====================================================
    // 5. Agregar como modos de ventana
    // =====================================================

    foreach (Vector2Int resolution in validResolutions)
    {
        AddPreset(
            resolution.x,
            resolution.y,
            FullScreenMode.Windowed,
            $"{resolution.x} x {resolution.y} (VENTANA)"
        );
    }
}
    
    private void AddCommonResolution(
        List<Vector2Int> list,
        int width,
        int height,
        int nativeWidth,
        int nativeHeight,
        float nativeAspect)
    {
        // No meter resoluciones mayores al monitor.
        if (width > nativeWidth || height > nativeHeight)
            return;


        float aspect =
            (float)width / height;

        // No meter resoluciones con otra proporción.
        if (Mathf.Abs(aspect - nativeAspect) > 0.03f)
            return;


        Vector2Int resolution =
            new Vector2Int(width, height);

        if (!list.Contains(resolution))
            list.Add(resolution);
    }


    // =========================================================
    // AGREGAR RESOLUCIÓN DE VENTANA
    // =========================================================

    private void AddWindowResolution(
        int width,
        int height)
    {
        int nativeWidth =
            Screen.currentResolution.width;

        int nativeHeight =
            Screen.currentResolution.height;


        // No agregamos resoluciones mayores
        // que el monitor.
        if (
            width > nativeWidth ||
            height > nativeHeight
        )
        {
            return;
        }


        AddPreset(
            width,
            height,
            FullScreenMode.Windowed,
            $"{width} x {height} (VENTANA)"
        );
    }


    // =========================================================
    // AGREGAR PRESET EVITANDO DUPLICADOS
    // =========================================================

    private void AddPreset(
        int width,
        int height,
        FullScreenMode mode,
        string displayName)
    {
        foreach (DisplayPreset preset in displayPresets)
        {
            if (
                preset.width == width &&
                preset.height == height &&
                preset.mode == mode
            )
            {
                return;
            }
        }

        displayPresets.Add(
            new DisplayPreset(
                width,
                height,
                mode,
                displayName
            )
        );
    }


    // =========================================================
    // CARGAR OPCIÓN GUARDADA
    // =========================================================

    private void LoadDisplayPreset()
    {
        int nativeWidth =
            Screen.currentResolution.width;

        int nativeHeight =
            Screen.currentResolution.height;


        // Primera vez:
        // resolución nativa + pantalla completa.
        int savedWidth =
            PlayerPrefs.GetInt(
                DISPLAY_WIDTH_KEY,
                nativeWidth
            );

        int savedHeight =
            PlayerPrefs.GetInt(
                DISPLAY_HEIGHT_KEY,
                nativeHeight
            );

        FullScreenMode savedMode =
            (FullScreenMode)PlayerPrefs.GetInt(
                DISPLAY_MODE_KEY,
                (int)FullScreenMode.FullScreenWindow
            );


        currentDisplayPreset = -1;


        // Buscamos la opción guardada.
        for (int i = 0; i < displayPresets.Count; i++)
        {
            DisplayPreset preset =
                displayPresets[i];

            if (
                preset.width == savedWidth &&
                preset.height == savedHeight &&
                preset.mode == savedMode
            )
            {
                currentDisplayPreset = i;
                break;
            }
        }


        // Si cambió de monitor o ya no existe
        // esa resolución:
        //
        // usamos la primera opción:
        // nativa + pantalla completa.
        if (currentDisplayPreset < 0)
        {
            currentDisplayPreset = 0;
        }


        ApplyCurrentDisplayPreset(false);
    }


    // =========================================================
    // CAMBIAR RESOLUCIÓN
    // =========================================================

    public void ChangeDisplayPreset(int direction)
    {
        if (
            displayPresets == null ||
            displayPresets.Count == 0
        )
        {
            return;
        }


        currentDisplayPreset += direction;


        if (currentDisplayPreset >= displayPresets.Count)
            currentDisplayPreset = 0;

        if (currentDisplayPreset < 0)
            currentDisplayPreset =
                displayPresets.Count - 1;


        ApplyCurrentDisplayPreset(true);
    }


    // =========================================================
    // APLICAR RESOLUCIÓN
    // =========================================================

    private void ApplyCurrentDisplayPreset(
        bool save)
    {
        if (
            displayPresets == null ||
            displayPresets.Count == 0
        )
        {
            return;
        }


        DisplayPreset preset =
            displayPresets[currentDisplayPreset];


        Screen.SetResolution(
            preset.width,
            preset.height,
            preset.mode
        );


        if (!save)
            return;


        PlayerPrefs.SetInt(
            DISPLAY_WIDTH_KEY,
            preset.width
        );

        PlayerPrefs.SetInt(
            DISPLAY_HEIGHT_KEY,
            preset.height
        );

        PlayerPrefs.SetInt(
            DISPLAY_MODE_KEY,
            (int)preset.mode
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // TEXTO PARA LA UI
    // =========================================================

    public string GetDisplayPresetName()
    {
        if (
            displayPresets == null ||
            displayPresets.Count == 0
        )
        {
            return "";
        }


        return
            displayPresets[currentDisplayPreset]
            .displayName;
    }


    // =========================================================
    // DATOS OPCIONALES
    // =========================================================

    public int GetCurrentDisplayPresetIndex()
    {
        return currentDisplayPreset;
    }

    public int GetDisplayPresetCount()
    {
        if (displayPresets == null)
            return 0;

        return displayPresets.Count;
    }
}