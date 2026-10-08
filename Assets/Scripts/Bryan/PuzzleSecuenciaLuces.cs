using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Puzzle de secuencia de luces.
/// Genera una secuencia aleatoria al comenzar.
/// El jugador debe memorizarla y repetirla.
/// </summary>
public class PuzzleSecuenciaLuces : MonoBehaviour
{
    // =========================================================
    // LUCES
    // =========================================================

    [Header("Luces")]
    [SerializeField] private LuzPuzzle[] luces;


    // =========================================================
    // SECUENCIA ALEATORIA
    // =========================================================

    [Header("Secuencia Aleatoria")]

    [Tooltip("Cantidad de luces que tendrá la secuencia.")]
    [SerializeField] private int longitudSecuencia = 4;

    [Tooltip("Si está activo, una misma luz puede aparecer varias veces.")]
    [SerializeField] private bool permitirRepetidas = false;

    [Tooltip("Si está activo, genera una secuencia nueva cada vez que el jugador falla.")]
    [SerializeField] private bool regenerarAlFallar = false;

    [Tooltip("Muestra la secuencia generada en la Console para hacer pruebas.")]
    [SerializeField] private bool mostrarSecuenciaEnConsola = true;

    private int[] secuenciaCorrecta;


    // =========================================================
    // PRESENTACION
    // =========================================================

    [Header("Presentacion")]

    [SerializeField]
    private bool reproducirSecuenciaAlIniciar = true;

    [SerializeField]
    private float duracionPistaEncendida = 0.55f;

    [SerializeField]
    private float duracionPistaApagada = 0.25f;

    [SerializeField]
    private float retrasoReinicio = 0.8f;


    // =========================================================
    // AL RESOLVER
    // =========================================================

    [Header("Al resolver")]

    [SerializeField]
    private GameObject puertaOBarrera;

    [SerializeField]
    private UnityEvent alResolver;


    // =========================================================
    // ENEMIGOS
    // =========================================================

    [Header("Enemigos")]

    [Tooltip("Mientras el puzzle no esté resuelto, estos generadores siguen creando enemigos.")]
    [SerializeField]
    private GeneradorEnemigos[] generadoresEnemigos;


    // =========================================================
    // ESTADO
    // =========================================================

    private int paso;

    private bool resuelto;

    private bool ocupado;


    public bool AceptaEntrada =>
        !resuelto && !ocupado;

    public bool Resuelto =>
        resuelto;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Configuramos cada luz.
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
            {
                luces[i].Configurar(this, i);
            }
        }


        // Generamos la secuencia random.
        GenerarSecuenciaAleatoria();


        // Reproducimos la pista.
        if (reproducirSecuenciaAlIniciar)
        {
            StartCoroutine(
                ReproducirSecuenciaPista()
            );
        }
        else
        {
            PonerTodasEnReposo();
        }
    }


    // =========================================================
    // GENERAR SECUENCIA ALEATORIA
    // =========================================================

    private void GenerarSecuenciaAleatoria()
    {
        if (luces == null ||
            luces.Length == 0)
        {
            Debug.LogError(
                "PuzzleSecuenciaLuces: No hay luces configuradas."
            );

            return;
        }


        // Guardamos solamente índices de luces válidas.
        List<int> indicesValidos =
            new List<int>();


        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
            {
                indicesValidos.Add(i);
            }
        }


        if (indicesValidos.Count == 0)
        {
            Debug.LogError(
                "PuzzleSecuenciaLuces: Todas las luces son NULL."
            );

            return;
        }


        // =====================================================
        // PERMITIENDO REPETICIONES
        // =====================================================

        if (permitirRepetidas)
        {
            int longitud =
                Mathf.Max(
                    1,
                    longitudSecuencia
                );


            secuenciaCorrecta =
                new int[longitud];


            for (int i = 0;
                 i < secuenciaCorrecta.Length;
                 i++)
            {
                int posicionRandom =
                    Random.Range(
                        0,
                        indicesValidos.Count
                    );


                secuenciaCorrecta[i] =
                    indicesValidos[posicionRandom];
            }
        }

        // =====================================================
        // SIN REPETICIONES
        // =====================================================

        else
        {
            int longitud =
                Mathf.Clamp(
                    longitudSecuencia,
                    1,
                    indicesValidos.Count
                );


            // Barajamos la lista.
            for (int i = 0;
                 i < indicesValidos.Count;
                 i++)
            {
                int random =
                    Random.Range(
                        i,
                        indicesValidos.Count
                    );


                int temporal =
                    indicesValidos[i];


                indicesValidos[i] =
                    indicesValidos[random];


                indicesValidos[random] =
                    temporal;
            }


            secuenciaCorrecta =
                new int[longitud];


            for (int i = 0;
                 i < longitud;
                 i++)
            {
                secuenciaCorrecta[i] =
                    indicesValidos[i];
            }
        }


        paso = 0;


        // Solo para debug.
        if (mostrarSecuenciaEnConsola)
        {
            Debug.Log(
                "Secuencia generada: " +
                string.Join(
                    " - ",
                    secuenciaCorrecta
                )
            );
        }
    }


    // =========================================================
    // REPETIR SECUENCIA
    // =========================================================

    public void RepetirSecuencia()
    {
        if (!resuelto &&
            !ocupado)
        {
            StartCoroutine(
                ReproducirSecuenciaPista()
            );
        }
    }


    // =========================================================
    // JUGADOR ACTIVA UNA LUZ
    // =========================================================

    public void IntentarActivar(
        LuzPuzzle luz
    )
    {
        if (!AceptaEntrada)
            return;


        if (luz == null)
            return;


        if (secuenciaCorrecta == null ||
            secuenciaCorrecta.Length == 0)
        {
            return;
        }


        if (paso >=
            secuenciaCorrecta.Length)
        {
            return;
        }


        int esperado =
            secuenciaCorrecta[paso];


        luz.MostrarSeleccionado();


        // =====================================================
        // ERROR
        // =====================================================

        if (luz.Indice != esperado)
        {
            StartCoroutine(
                GestionarError()
            );

            return;
        }


        // =====================================================
        // ACIERTO
        // =====================================================

        paso++;


        // Completó toda la secuencia.
        if (paso >=
            secuenciaCorrecta.Length)
        {
            GestionarAcierto();
        }
    }


    // =========================================================
    // MOSTRAR SECUENCIA
    // =========================================================

    private IEnumerator ReproducirSecuenciaPista()
    {
        ocupado = true;

        paso = 0;


        ApagarTodas();


        yield return new WaitForSeconds(
            0.4f
        );


        for (int i = 0;
             i < secuenciaCorrecta.Length;
             i++)
        {
            int indice =
                secuenciaCorrecta[i];


            if (!EsIndiceValido(indice))
                continue;


            luces[indice]
                .MostrarPista();


            yield return new WaitForSeconds(
                duracionPistaEncendida
            );


            luces[indice]
                .MostrarApagado();


            yield return new WaitForSeconds(
                duracionPistaApagada
            );
        }


        PonerTodasEnReposo();

        ocupado = false;
    }


    // =========================================================
    // ERROR
    // =========================================================

    private IEnumerator GestionarError()
    {
        ocupado = true;


        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
            {
                luz.MostrarError();
            }
        }


        yield return new WaitForSeconds(
            retrasoReinicio
        );


        // Si quieres que cada error genere
        // una secuencia completamente nueva.
        if (regenerarAlFallar)
        {
            GenerarSecuenciaAleatoria();
        }


        yield return
            ReproducirSecuenciaPista();
    }


    // =========================================================
    // RESUELTO
    // =========================================================

    private void GestionarAcierto()
    {
        ocupado = true;

        resuelto = true;


        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
            {
                luz.MostrarAcierto();
            }
        }


        if (puertaOBarrera != null)
        {
            puertaOBarrera.SetActive(true);
        }


        Debug.Log(
            "Sí lo hiciste bien bb, good boy"
        );


        DetenerGeneradores();


        alResolver?.Invoke();
    }


    // =========================================================
    // REPOSO
    // =========================================================

    private void PonerTodasEnReposo()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
            {
                luz.MostrarReposo();
            }
        }
    }


    // =========================================================
    // APAGAR TODAS
    // =========================================================

    private void ApagarTodas()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
            {
                luz.MostrarApagado();
            }
        }
    }


    // =========================================================
    // DETENER GENERADORES
    // =========================================================

    private void DetenerGeneradores()
    {
        if (generadoresEnemigos == null)
            return;


        foreach (
            GeneradorEnemigos generador
            in generadoresEnemigos
        )
        {
            if (generador != null)
            {
                generador
                    .DetenerGeneracion();
            }
        }
    }


    // =========================================================
    // VALIDAR INDICE
    // =========================================================

    private bool EsIndiceValido(
        int indice
    )
    {
        return
            indice >= 0 &&
            indice < luces.Length &&
            luces[indice] != null;
    }
}