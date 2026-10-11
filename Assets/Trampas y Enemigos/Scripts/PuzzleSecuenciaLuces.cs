using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// Puzzle de secuencia de luces.
/// Genera una secuencia aleatoria al comenzar.
/// El jugador debe memorizarla y repetirla.
public class PuzzleSecuenciaLuces : MonoBehaviour
{
    // LUCES
    [Header("Luces")]
    [SerializeField] private LuzPuzzle[] luces;
    // PLACA DE PRESION
    [Header("Placa de Presion")]
    [Tooltip("Si esta activo, el jugador no podra interactuar con las luces hasta pisar la placa.")]
    [SerializeField] private bool requierePlacaParaActivar = true;
    private bool habilitadoPorPlaca = false;

    // SECUENCIA ALEATORIA

    [Header("Secuencia Aleatoria")]

    [Tooltip("Cantidad de luces que tendra la secuencia.")]
    [SerializeField] private int longitudSecuencia = 4;

    [Tooltip("Si esta activo, una misma luz puede aparecer varias veces.")]
    [SerializeField] private bool permitirRepetidas = false;

    [Tooltip("Si esta activo, genera una secuencia nueva cada vez que el jugador falla.")]
    [SerializeField] private bool regenerarAlFallar = false;

    [Tooltip("Muestra la secuencia generada en la Console para hacer pruebas.")]
    [SerializeField] private bool mostrarSecuenciaEnConsola = true;

    private int[] secuenciaCorrecta;

    // PRESENTACION
    [Header("Presentacion")]

    [SerializeField]
    private bool reproducirSecuenciaAlIniciar = false; // Desactivado por defecto si usa placa

    [SerializeField]
    private float duracionPistaEncendida = 0.55f;

    [SerializeField]
    private float duracionPistaApagada = 0.25f;

    [SerializeField]
    private float retrasoReinicio = 0.8f;

    // AL RESOLVER

    [Header("Al resolver")]

    [SerializeField]
    private GameObject puertaOBarrera;

    [SerializeField]
    private UnityEvent alResolver;

    // ENEMIGOS

    [Header("Enemigos")]

    [Tooltip("Mientras el puzzle no este resuelto, estos generadores siguen creando enemigos.")]
    [SerializeField]
    private GeneradorEnemigos[] generadoresEnemigos;

    // ESTADO

    private int paso;
    private bool resuelto;
    private bool ocupado;

    private Coroutine reproduccionActiva;
    private Coroutine gestionErrorActiva;

    // AHORA VALIDA SI LA PLACA YA FUE ACTIVADA
    public bool AceptaEntrada => (!requierePlacaParaActivar || habilitadoPorPlaca) && !resuelto && !ocupado;

    public bool Resuelto => resuelto;
    // START
    private void Start()
    {
        if (luces == null || luces.Length == 0)
        {
            Debug.LogError(
                "PuzzleSecuenciaLuces: No hay luces configuradas.",
                this);

            return;
        }

        // Configuramos cada luz.
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
                luces[i].Configurar(this, i);
        }

        // Generamos la secuencia aleatoria en memoria desde el inicio.
        GenerarSecuenciaAleatoria();

        // Reproducimos la pista inicial solo si no requiere placa.
        if (!requierePlacaParaActivar && reproducirSecuenciaAlIniciar && secuenciaCorrecta != null)
        {
            IniciarReproduccion();
        }
        else
        {
            PonerTodasEnReposo();
        }
    }
    // ACTIVACION DESDE LA PLACA DE PRESION
    public void ActivarDesdePlaca()
    {
        if (resuelto) return;

        // Solo desbloqueamos el permiso, NO reproducimos la secuencia todavia
        habilitadoPorPlaca = true;
        Debug.Log("Puzzle desbloqueado por la placa. Esperando a que el jugador se acerque a los totems.");
    }
    // DETECCION DE CERCANIA A LOS TOTEMS
    public void JugadorEntroEnZonaVision()
    {
        if (resuelto || ocupado) return;

        // Si la placa ya fue pisada y la secuencia no se esta mostrando, la reproducimos ahora
        if (habilitadoPorPlaca && reproduccionActiva == null)
        {
            IniciarReproduccion();
        }
    }

    // GENERAR SECUENCIA ALEATORIA
    private void GenerarSecuenciaAleatoria()
    {
        if (luces == null || luces.Length == 0)
        {
            Debug.LogError(
                "PuzzleSecuenciaLuces: No hay luces configuradas.",
                this);

            secuenciaCorrecta = null;
            return;
        }

        List<int> indicesValidos = new List<int>();

        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
                indicesValidos.Add(i);
        }

        if (indicesValidos.Count == 0)
        {
            Debug.LogError(
                "PuzzleSecuenciaLuces: Todas las luces son NULL.",
                this);

            secuenciaCorrecta = null;
            return;
        }

        if (permitirRepetidas)
        {
            int longitud = Mathf.Max(1, longitudSecuencia);
            secuenciaCorrecta = new int[longitud];

            for (int i = 0; i < secuenciaCorrecta.Length; i++)
            {
                int posicionRandom = Random.Range(0, indicesValidos.Count);
                secuenciaCorrecta[i] = indicesValidos[posicionRandom];
            }
        }
        else
        {
            int longitud = Mathf.Clamp(
                longitudSecuencia,
                1,
                indicesValidos.Count);

            for (int i = 0; i < indicesValidos.Count; i++)
            {
                int random = Random.Range(i, indicesValidos.Count);
                int temporal = indicesValidos[i];
                indicesValidos[i] = indicesValidos[random];
                indicesValidos[random] = temporal;
            }

            secuenciaCorrecta = new int[longitud];

            for (int i = 0; i < longitud; i++)
                secuenciaCorrecta[i] = indicesValidos[i];
        }

        paso = 0;

        if (mostrarSecuenciaEnConsola)
        {
            Debug.Log(
                "Secuencia generada: " +
                string.Join(" - ", secuenciaCorrecta),
                this);
        }
    }

    // REPETIR SECUENCIA

    public void RepetirSecuencia()
    {
        if (resuelto || ocupado)
            return;

        IniciarReproduccion();
    }

    private void IniciarReproduccion()
    {
        if (resuelto ||
            reproduccionActiva != null ||
            secuenciaCorrecta == null ||
            secuenciaCorrecta.Length == 0)
        {
            return;
        }

        reproduccionActiva =
            StartCoroutine(ReproducirSecuenciaPista());
    }

    // JUGADOR ACTIVA UNA LUZ
    public void IntentarActivar(LuzPuzzle luz)
    {
        if (!AceptaEntrada || luz == null)
            return;

        if (secuenciaCorrecta == null ||
            secuenciaCorrecta.Length == 0)
        {
            return;
        }

        if (paso >= secuenciaCorrecta.Length)
            return;

        int esperado = secuenciaCorrecta[paso];

        luz.MostrarSeleccionado();

        // ERROR
        if (luz.Indice != esperado)
        {
            if (gestionErrorActiva == null)
            {
                gestionErrorActiva =
                    StartCoroutine(GestionarError());
            }

            return;
        }

        // ACIERTO
        paso++;

        if (paso >= secuenciaCorrecta.Length)
            GestionarAcierto();
    }

    // MOSTRAR SECUENCIA
    private IEnumerator ReproducirSecuenciaPista()
    {
        ocupado = true;
        paso = 0;

        ApagarTodas();

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < secuenciaCorrecta.Length; i++)
        {
            if (resuelto || !isActiveAndEnabled)
                break;

            int indice = secuenciaCorrecta[i];

            if (!EsIndiceValido(indice))
                continue;

            luces[indice].MostrarPista();

            yield return new WaitForSeconds(duracionPistaEncendida);

            luces[indice].MostrarApagado();

            yield return new WaitForSeconds(duracionPistaApagada);
        }

        if (!resuelto && isActiveAndEnabled)
            PonerTodasEnReposo();

        ocupado = resuelto;
        reproduccionActiva = null;
    }

    // ERROR
    private IEnumerator GestionarError()
    {
        ocupado = true;

        if (reproduccionActiva != null)
        {
            StopCoroutine(reproduccionActiva);
            reproduccionActiva = null;
        }

        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarError();
        }

        yield return new WaitForSeconds(retrasoReinicio);

        if (resuelto || !isActiveAndEnabled)
        {
            gestionErrorActiva = null;
            yield break;
        }

        ApagarTodas();

        if (regenerarAlFallar)
            GenerarSecuenciaAleatoria();

        gestionErrorActiva = null;

        reproduccionActiva =
            StartCoroutine(ReproducirSecuenciaPista());
    }

    // RESUELTO
    private void GestionarAcierto()
    {
        ocupado = true;
        resuelto = true;

        if (reproduccionActiva != null)
        {
            StopCoroutine(reproduccionActiva);
            reproduccionActiva = null;
        }

        if (gestionErrorActiva != null)
        {
            StopCoroutine(gestionErrorActiva);
            gestionErrorActiva = null;
        }

        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarAcierto();
        }

        if (puertaOBarrera != null)
            puertaOBarrera.SetActive(true);

        Debug.Log("Puzzle resuelto con exito.", this);

        DetenerGeneradores();

        alResolver?.Invoke();
    }

    // REPOSO
    private void PonerTodasEnReposo()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarReposo();
        }
    }

    // APAGAR TODAS
    private void ApagarTodas()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarApagado();
        }
    }

    // DETENER GENERADORES
    private void DetenerGeneradores()
    {
        if (generadoresEnemigos == null)
            return;

        foreach (GeneradorEnemigos generador in generadoresEnemigos)
        {
            if (generador != null)
                generador.DetenerGeneracion();
        }
    }

    // VALIDAR INDICE
    private bool EsIndiceValido(int indice)
    {
        return luces != null &&
               indice >= 0 &&
               indice < luces.Length &&
               luces[indice] != null;
    }

    // LIMPIEZA
    private void OnDisable()
    {
        if (reproduccionActiva != null)
        {
            StopCoroutine(reproduccionActiva);
            reproduccionActiva = null;
        }

        if (gestionErrorActiva != null)
        {
            StopCoroutine(gestionErrorActiva);
            gestionErrorActiva = null;
        }

        if (!resuelto)
            ocupado = false;
    }
}