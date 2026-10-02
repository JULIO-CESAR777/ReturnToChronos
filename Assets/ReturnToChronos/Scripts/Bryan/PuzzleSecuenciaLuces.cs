using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Puzzle de secuencia: muestra el orden de luces y el jugador debe repetirlo.
/// Si acierta, abre el paso al siguiente nivel.
/// </summary>
public class PuzzleSecuenciaLuces : MonoBehaviour
{
    [Header("Luces")]
    [SerializeField] LuzPuzzle[] luces;
    [Tooltip("Indices de las luces en el orden correcto. 0 es la primera luz del array.")]
    [SerializeField] int[] secuenciaCorrecta = { 0, 2, 1, 3 };

    [Header("Presentacion")]
    [SerializeField] bool reproducirSecuenciaAlIniciar = true;
    [SerializeField] float duracionPistaEncendida = 0.55f;
    [SerializeField] float duracionPistaApagada = 0.25f;
    [SerializeField] float retrasoReinicio = 0.8f;

    [Header("Siguiente nivel")]
    [SerializeField] GameObject puertaOBarrera;
    [SerializeField] string nombreSiguienteEscena;
    [SerializeField] float retrasoCargarSiguienteNivel = 1.5f;
    [SerializeField] UnityEvent alResolver;

    [Header("Enemigos")]
    [Tooltip("Mientras el puzzle no este resuelto, estos generadores siguen creando enemigos.")]
    [SerializeField] GeneradorEnemigos[] generadoresEnemigos;

    int paso;
    bool resuelto;
    bool ocupado;

    public bool AceptaEntrada => !resuelto && !ocupado;
    public bool Resuelto => resuelto;

    void Start()
    {
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
                luces[i].Configurar(this, i);
        }

        if (reproducirSecuenciaAlIniciar)
            StartCoroutine(ReproducirSecuenciaPista());
        else
            PonerTodasEnReposo();
    }

    public void RepetirSecuencia()
    {
        if (!resuelto)
            StartCoroutine(ReproducirSecuenciaPista());
    }

    public void IntentarActivar(LuzPuzzle luz)
    {
        if (!AceptaEntrada || luz == null || secuenciaCorrecta == null || secuenciaCorrecta.Length == 0)
            return;

        if (paso >= secuenciaCorrecta.Length)
            return;

        int esperado = secuenciaCorrecta[paso];
        luz.MostrarSeleccionado();

        if (luz.Indice != esperado)
        {
            StartCoroutine(GestionarError());
            return;
        }

        paso++;
        if (paso >= secuenciaCorrecta.Length)
            StartCoroutine(GestionarAcierto());
    }

    IEnumerator ReproducirSecuenciaPista()
    {
        ocupado = true;
        paso = 0;
        ApagarTodas();
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < secuenciaCorrecta.Length; i++)
        {
            int indice = secuenciaCorrecta[i];
            if (!EsIndiceValido(indice))
                continue;

            luces[indice].MostrarPista();
            yield return new WaitForSeconds(duracionPistaEncendida);
            luces[indice].MostrarApagado();
            yield return new WaitForSeconds(duracionPistaApagada);
        }

        PonerTodasEnReposo();
        ocupado = false;
    }

    IEnumerator GestionarError()
    {
        ocupado = true;
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarError();
        }

        yield return new WaitForSeconds(retrasoReinicio);
        yield return ReproducirSecuenciaPista();
    }

    IEnumerator GestionarAcierto()
    {
        ocupado = true;
        resuelto = true;

        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarAcierto();
        }

        if (puertaOBarrera != null)
            puertaOBarrera.SetActive(false);

        DetenerGeneradores();
        alResolver?.Invoke();

        if (!string.IsNullOrWhiteSpace(nombreSiguienteEscena))
        {
            yield return new WaitForSeconds(retrasoCargarSiguienteNivel);
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
    }

    void PonerTodasEnReposo()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarReposo();
        }
    }

    void ApagarTodas()
    {
        foreach (LuzPuzzle luz in luces)
        {
            if (luz != null)
                luz.MostrarApagado();
        }
    }

    void DetenerGeneradores()
    {
        if (generadoresEnemigos == null)
            return;

        foreach (GeneradorEnemigos generador in generadoresEnemigos)
        {
            if (generador != null)
                generador.DetenerGeneracion();
        }
    }

    bool EsIndiceValido(int indice)
    {
        return indice >= 0 && indice < luces.Length && luces[indice] != null;
    }
}
