
using UnityEngine;
using UnityEngine.UI;

public class OscuridadVacio : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Collider[] limitesMapa;
    [SerializeField] private Image pantallaNegra;

    [Header("Distancias")]
    [SerializeField] private float distanciaInicio = 8f;
    [SerializeField] private float distanciaOscuridadTotal = 0.7f;
    [SerializeField] private float alturaDeteccion = 1f;

    [Header("Deteccion")]
    [SerializeField, Range(8, 64)]
    private int cantidadRayos = 32;

    [SerializeField] private bool mostrarRayos = false;

    [Header("Transicion")]
    [Range(0f, 1f)]
    [SerializeField] private float opacidadMaxima = 1f;

    [SerializeField] private float velocidadTransicion = 1.5f;

    private float opacidadActual;

    private void Awake()
    {
        if (pantallaNegra == null)
            return;

        pantallaNegra.raycastTarget = false;
        EstablecerOpacidad(0f);
    }

    private void Update()
    {
        if (jugador == null || pantallaNegra == null)
            return;

        float distanciaMinima = DetectarDistancia();

        float rango = Mathf.Max(
            0.01f,
            distanciaInicio - distanciaOscuridadTotal
        );

        float factor = Mathf.Clamp01(
            (distanciaInicio - distanciaMinima) / rango
        );

        // Suavizado de la oscuridad
        factor = factor * factor * (3f - 2f * factor);

        float opacidadObjetivo = factor * opacidadMaxima;

        opacidadActual = Mathf.MoveTowards(
            opacidadActual,
            opacidadObjetivo,
            velocidadTransicion * Time.deltaTime
        );

        EstablecerOpacidad(opacidadActual);
    }

    private float DetectarDistancia()
    {
        float distanciaMinima = distanciaInicio;

        Vector3 origen = jugador.position +
                          Vector3.up * alturaDeteccion;

        for (int i = 0; i < cantidadRayos; i++)
        {
            float angulo = i * 360f / cantidadRayos;

            float radianes = angulo * Mathf.Deg2Rad;

            Vector3 direccion = new Vector3(
                Mathf.Cos(radianes),
                0f,
                Mathf.Sin(radianes)
            );

            Ray rayo = new Ray(origen, direccion);

            foreach (Collider limite in limitesMapa)
            {
                if (limite == null ||
                    !limite.enabled ||
                    !limite.gameObject.activeInHierarchy)
                    continue;

                if (limite.Raycast(
                    rayo,
                    out RaycastHit impacto,
                    distanciaInicio))
                {
                    if (impacto.distance < distanciaMinima)
                    {
                        distanciaMinima = impacto.distance;
                    }
                }
            }

            if (mostrarRayos)
            {
                Debug.DrawRay(
                    origen,
                    direccion * distanciaInicio,
                    Color.red
                );
            }
        }

        return distanciaMinima;
    }

    private void EstablecerOpacidad(float opacidad)
    {
        if (pantallaNegra == null)
            return;

        pantallaNegra.color = new Color(
            0f,
            0f,
            0f,
            opacidad
        );
    }

    private void OnDisable()
    {
        opacidadActual = 0f;
        EstablecerOpacidad(0f);
    }
}
