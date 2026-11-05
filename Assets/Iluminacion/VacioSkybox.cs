
using UnityEngine;

public class VacioInfernalSkybox : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Collider[] limitesMapa;

    [Header("Distancias")]
    [SerializeField] private float distanciaInicio = 15f;
    [SerializeField] private float distanciaNegroTotal = 2f;
    [SerializeField] private float alturaDeteccion = 1f;

    [Header("Deteccion")]
    [SerializeField, Range(8, 128)]
    private int cantidadRayos = 48;

    [SerializeField] private bool mostrarRayos = false;

    [Header("Transicion del cielo")]
    [SerializeField] private float velocidadTransicion = 2f;

    private Material skyboxOriginal;
    private Material skyboxInstancia;

    private float exposicionOriginal;
    private float oscuridadActual;

    private static readonly int ExposureID =
        Shader.PropertyToID("_Exposure");

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        PrepararSkybox();
    }

    private void Update()
    {
        if (jugador == null)
            return;

        // Detectar cambios de Skybox entre pisos
        if (RenderSettings.skybox != skyboxInstancia)
        {
            PrepararSkybox();
        }

        if (skyboxInstancia == null)
            return;

        float distancia = ObtenerDistanciaAlBorde();

        float rango = Mathf.Max(
            0.01f,
            distanciaInicio - distanciaNegroTotal
        );

        float factor = Mathf.Clamp01(
            (distanciaInicio - distancia) / rango
        );

        factor = Mathf.SmoothStep(0f, 1f, factor);

        // Transicion temporal suave
        oscuridadActual = Mathf.MoveTowards(
            oscuridadActual,
            factor,
            velocidadTransicion * Time.deltaTime
        );

        float exposicionFinal = Mathf.Lerp(
            exposicionOriginal,
            0f,
            oscuridadActual
        );

        skyboxInstancia.SetFloat(
            ExposureID,
            exposicionFinal
        );
    }

    private float ObtenerDistanciaAlBorde()
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
                    distanciaMinima = Mathf.Min(
                        distanciaMinima,
                        impacto.distance
                    );
                }
            }

            if (mostrarRayos)
            {
                Debug.DrawRay(
                    origen,
                    direccion * distanciaInicio,
                    Color.magenta
                );
            }
        }

        return distanciaMinima;
    }

    private void PrepararSkybox()
    {
        if (skyboxInstancia != null)
        {
            Destroy(skyboxInstancia);
            skyboxInstancia = null;
        }

        Material actual = RenderSettings.skybox;

        if (actual == null)
            return;

        if (!actual.HasProperty(ExposureID))
        {
            Debug.LogWarning(
                "El Skybox actual no tiene la propiedad _Exposure."
            );
            enabled = false;
            return;
        }

        skyboxOriginal = actual;
        exposicionOriginal = actual.GetFloat(ExposureID);

        // Copia temporal para no modificar el material original
        skyboxInstancia = new Material(actual);
        skyboxInstancia.name = actual.name + "_Runtime";

        RenderSettings.skybox = skyboxInstancia;

        oscuridadActual = 0f;
    }

    private void OnDisable()
    {
        if (skyboxInstancia == null)
            return;

        if (RenderSettings.skybox == skyboxInstancia)
        {
            RenderSettings.skybox = skyboxOriginal;
        }

        Destroy(skyboxInstancia);
        skyboxInstancia = null;
    }
}
