using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camara 2.5D tipo Don't Starve: angulo fijo ligeramente top-down y seguimiento del jugador.
/// Zoom: rueda del raton, D-pad arriba/abajo, o LB/RB.
/// </summary>
public class CamaraSeguirJugador : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] Transform objetivo;
    [SerializeField] bool buscarJugadorPorTag = true;

    [Header("Posicion")]
    [Tooltip("Desplazamiento respecto al jugador. Ejemplo top-down suave: (0, 12, -10).")]
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, -10f);
    [SerializeField] bool seguirEjeX = true;
    [SerializeField] bool seguirEjeY = true;
    [SerializeField] bool seguirEjeZ = true;
    [SerializeField] float suavizadoPosicion = 8f;

    [Header("Rotacion")]
    [Tooltip("Si esta activo, la camara mantiene un angulo fijo y el mundo no gira.")]
    [SerializeField] bool usarRotacionFija = true;
    [SerializeField] Vector3 rotacionFija = new Vector3(50f, 0f, 0f);
    [SerializeField] bool mirarAlObjetivo;
    [SerializeField] Vector3 offsetMirada;
    [SerializeField] float suavizadoRotacion = 8f;

    [Header("Zoom")]
    [SerializeField] bool permitirZoom = true;
    [SerializeField] float velocidadZoomRaton = 4f;
    [SerializeField] float velocidadZoomMando = 8f;
    [SerializeField] float zoomMinimo = 6f;
    [SerializeField] float zoomMaximo = 18f;
    [SerializeField] bool zoomConDpad = true;
    [SerializeField] bool zoomConHombros = true;

    Vector3 velocidadActual;

    void Awake()
    {
        if (objetivo == null && buscarJugadorPorTag)
        {
            GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
            if (encontrado != null)
                objetivo = encontrado.transform;
        }
    }

    void LateUpdate()
    {
        if (objetivo == null)
            return;

        AplicarZoom();
        MoverCamara();
        RotarCamara();
    }

    void MoverCamara()
    {
        Vector3 destino = objetivo.position + offset;
        Vector3 actual = transform.position;

        if (!seguirEjeX) destino.x = actual.x;
        if (!seguirEjeY) destino.y = actual.y;
        if (!seguirEjeZ) destino.z = actual.z;

        if (suavizadoPosicion <= 0f)
            transform.position = destino;
        else
            transform.position = Vector3.SmoothDamp(actual, destino, ref velocidadActual, 1f / suavizadoPosicion);
    }

    void RotarCamara()
    {
        Quaternion rotacionDeseada = transform.rotation;

        if (usarRotacionFija)
            rotacionDeseada = Quaternion.Euler(rotacionFija);
        else if (mirarAlObjetivo)
            rotacionDeseada = Quaternion.LookRotation((objetivo.position + offsetMirada) - transform.position);

        if (suavizadoRotacion <= 0f)
            transform.rotation = rotacionDeseada;
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, suavizadoRotacion * Time.deltaTime);
    }

    void AplicarZoom()
    {
        if (!permitirZoom)
            return;

        float cambio = 0f;

        Mouse raton = Mouse.current;
        if (raton != null)
            cambio += raton.scroll.ReadValue().y * velocidadZoomRaton * 0.05f;

        Gamepad mando = Gamepad.current;
        if (mando != null)
        {
            float pasoMando = velocidadZoomMando * Time.deltaTime;

            if (zoomConDpad)
            {
                if (mando.dpad.up.isPressed) cambio += pasoMando;
                if (mando.dpad.down.isPressed) cambio -= pasoMando;
            }

            if (zoomConHombros)
            {
                if (mando.leftShoulder.isPressed) cambio += pasoMando;
                if (mando.rightShoulder.isPressed) cambio -= pasoMando;
            }
        }

        if (Mathf.Abs(cambio) < 0.0001f)
            return;

        float factor = 1f - cambio;
        Vector3 nuevoOffset = offset * factor;
        float distancia = Mathf.Clamp(nuevoOffset.magnitude, zoomMinimo, zoomMaximo);
        if (nuevoOffset.sqrMagnitude < 0.001f)
            return;

        offset = nuevoOffset.normalized * distancia;
    }

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }

    public void EstablecerOffset(Vector3 nuevoOffset)
    {
        offset = nuevoOffset;
    }
}
