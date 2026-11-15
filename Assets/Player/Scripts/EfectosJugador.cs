using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Audio;

public class EfectosJugador : MonoBehaviour
{
    [Header("Aterrizaje")]
    [SerializeField] private Animator visual;
    [Range(0.7f, 1f)] [SerializeField] private float escalaAlAterrizar = 0.87f;
    [SerializeField] private Material materialParticulas;

    [Header("Combate")]
    [Range(0f, 0.1f)] [SerializeField] private float pausaImpacto = 0.04f;
    [SerializeField] private AudioClip sonidoAtaque;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private AudioMixerGroup salidaEfectos;

    private MMF_Player aterrizaje;
    private ParticleSystem particulas;
    private AudioSource aire;
    private AudioSource impacto;
    private Coroutine pausa;
    private float velocidadAnimacion;

    private void Awake()
    {
        var soporte = new GameObject("Efectos de movimiento y combate");
        soporte.transform.SetParent(transform, false);
        aterrizaje = soporte.AddComponent<MMF_Player>();
        var compresion = (MMF_SquashAndStretch)aterrizaje.AddFeedback(typeof(MMF_SquashAndStretch));
        compresion.SquashAndStretchTarget = visual.transform;
        compresion.AnimateScaleDuration = 0.11f;
        compresion.RemapCurveOne = escalaAlAterrizar;
        compresion.AnimateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.22f, 1), new Keyframe(1, 0));
        aterrizaje.Initialization();

        particulas = soporte.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particulas.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 160;
        var emision = particulas.emission;
        emision.enabled = false;
        var forma = particulas.shape;
        forma.enabled = false;
        var tamano = particulas.sizeOverLifetime;
        tamano.enabled = true;
        tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0));
        soporte.GetComponent<ParticleSystemRenderer>().sharedMaterial = materialParticulas;
        particulas.Play();

        aire = CrearFuente(soporte);
        impacto = CrearFuente(soporte);
    }

    private AudioSource CrearFuente(GameObject soporte)
    {
        var fuente = soporte.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.outputAudioMixerGroup = salidaEfectos;
        return fuente;
    }

    public void Aterrizar(Vector3 pies, Vector3 velocidad)
    {
        aterrizaje.PlayFeedbacks();
        float fuerza = Mathf.Clamp01(velocidad.magnitude / 10f);
        int cantidad = Mathf.RoundToInt(Mathf.Lerp(4, 12, fuerza));
        for (int i = 0; i < cantidad; i++)
        {
            Vector2 radial = Random.insideUnitCircle;
            Vector3 salida = new Vector3(radial.x, 0.35f, radial.y);
            Emitir(pies + salida * 0.18f, salida - velocidad * 0.16f,
                new Color(0.72f, 0.64f, 0.53f, 0.65f), Random.Range(0.16f, 0.32f), 0.30f);
        }
    }

    public void SonarAtaque()
    {
        aire.pitch = Random.Range(0.95f, 1.05f);
        aire.PlayOneShot(sonidoAtaque, 0.24f);
    }

    public void Chispas(Vector3 punto, bool remate)
    {
        Vector3 direccion = Vector3.ProjectOnPlane(punto - transform.position, Vector3.up).normalized;
        for (int i = 0; i < (remate ? 10 : 6); i++)
        {
            Vector3 salida = direccion * 2f + Vector3.up + Random.insideUnitSphere * 2f;
            Emitir(punto, salida, new Color(1f, 0.78f, 0.36f), Random.Range(0.08f, 0.16f), 0.17f);
        }
    }

    private void Emitir(Vector3 origen, Vector3 velocidad, Color color, float tamano, float vida)
    {
        var datos = new ParticleSystem.EmitParams {
            position = origen, velocity = velocidad, startColor = color, startSize = tamano, startLifetime = vida
        };
        particulas.Emit(datos, 1);
    }

    public void ConfirmarGolpe(bool remate)
    {
        impacto.pitch = remate ? 0.76f : 1.05f;
        impacto.PlayOneShot(sonidoImpacto, remate ? 0.65f : 0.45f);
        if (pausa == null && pausaImpacto > 0f) pausa = StartCoroutine(PausarAnimacion());
        if (remate && Camera.main != null && Camera.main.TryGetComponent(out CamaraIsometrica camara))
            camara.SacudirPorRemate();
    }

    private IEnumerator PausarAnimacion()
    {
        velocidadAnimacion = visual.speed;
        visual.speed = 0f;
        yield return new WaitForSecondsRealtime(pausaImpacto);
        visual.speed = velocidadAnimacion;
        pausa = null;
    }

    private void OnDisable()
    {
        if (pausa == null) return;
        StopCoroutine(pausa);
        visual.speed = velocidadAnimacion;
        pausa = null;
    }
}
