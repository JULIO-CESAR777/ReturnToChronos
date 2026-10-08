using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CamaraIsometrica : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform objetivo;


    // =========================================================
    // POSICION
    // =========================================================

    [Header("Posicion")]

    [SerializeField]
    private Vector3 offsetBase =
        new Vector3(0f, 10f, -10f);

    [SerializeField]
    private Vector3 offsetMirada =
        new Vector3(0f, 1f, 0f);


    // =========================================================
    // SEGUIMIENTO
    // =========================================================

    [Header("Seguimiento")]

    [SerializeField]
    private float suavizadoPosicion = 8f;


    // =========================================================
    // ROTACION
    // =========================================================

    [Header("Rotacion")]

    [Tooltip("45 da 8 vistas. 90 da 4 vistas.")]
    [SerializeField]
    private float gradosPorGiro = 45f;

    [SerializeField]
    private float suavizadoRotacion = 10f;


    // =========================================================
    // ZOOM
    // =========================================================

    [Header("Zoom")]

    [Tooltip("Zoom mas cercano.")]
    [SerializeField]
    private float zoomMinimo = 3.5f;

    [Tooltip("Zoom mas lejano.")]
    [SerializeField]
    private float zoomMaximo = 9f;

    [Tooltip("Cantidad que cambia cada paso de la rueda.")]
    [SerializeField]
    private float pasoZoomRueda = 0.8f;

    [Tooltip("Cantidad que cambia cada pulsacion del D-Pad.")]
    [SerializeField]
    private float pasoZoomDPad = 0.8f;

    [Tooltip("Que tan rapido llega al nuevo zoom.")]
    [SerializeField]
    private float suavizadoZoom = 8f;


    // =========================================================
    // CONTROLES
    // =========================================================

    [Header("Controles")]

    [SerializeField]
    private bool usarQE = true;

    [SerializeField]
    private bool usarBotonesMando = true;

    [SerializeField]
    private bool usarRuedaMouse = true;

    [SerializeField]
    private bool usarDPadZoom = true;


    // =========================================================
    // VARIABLES
    // =========================================================

    private Camera camara;

    private float anguloObjetivo;
    private float anguloActual;

    private float zoomObjetivo;
    private float zoomActual;

    private float distanciaBase;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        camara = GetComponent<Camera>();

        distanciaBase = offsetBase.magnitude;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        anguloObjetivo = 0f;
        anguloActual = 0f;


        // -----------------------------------------------------
        // CAMARA ORTOGRAFICA
        // -----------------------------------------------------

        if (camara.orthographic)
        {
            zoomActual = camara.orthographicSize;

            zoomObjetivo = zoomActual;
        }

        // -----------------------------------------------------
        // CAMARA PERSPECTIVA
        // -----------------------------------------------------

        else
        {
            zoomActual = distanciaBase;

            zoomObjetivo = distanciaBase;

            zoomMinimo = Mathf.Min(
                zoomMinimo,
                distanciaBase
            );

            zoomMaximo = Mathf.Max(
                zoomMaximo,
                distanciaBase
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        LeerRotacion();

        LeerZoom();
    }


    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (objetivo == null)
            return;


        // =====================================================
        // ROTACION SUAVE
        // =====================================================

        anguloActual =
            Mathf.LerpAngle(
                anguloActual,
                anguloObjetivo,
                suavizadoRotacion *
                Time.deltaTime
            );


        Quaternion rotacionHorizontal =
            Quaternion.Euler(
                0f,
                anguloActual,
                0f
            );


        // =====================================================
        // ZOOM SUAVE
        // =====================================================

        zoomActual =
            Mathf.Lerp(
                zoomActual,
                zoomObjetivo,
                suavizadoZoom *
                Time.deltaTime
            );


        Vector3 offsetUsado;


        // =====================================================
        // ORTOGRAFICA
        // =====================================================

        if (camara.orthographic)
        {
            camara.orthographicSize =
                zoomActual;

            offsetUsado =
                offsetBase;
        }

        // =====================================================
        // PERSPECTIVA
        // =====================================================

        else
        {
            float multiplicador =
                zoomActual /
                distanciaBase;

            offsetUsado =
                offsetBase *
                multiplicador;
        }


        // =====================================================
        // ROTAMOS EL OFFSET
        // =====================================================

        Vector3 offsetRotado =
            rotacionHorizontal *
            offsetUsado;


        Vector3 posicionDeseada =
            objetivo.position +
            offsetRotado;


        // =====================================================
        // SEGUIMIENTO
        // =====================================================

        transform.position =
            Vector3.Lerp(
                transform.position,
                posicionDeseada,
                suavizadoPosicion *
                Time.deltaTime
            );


        // =====================================================
        // MIRAR AL JUGADOR
        // =====================================================

        Vector3 puntoMirada =
            objetivo.position +
            offsetMirada;


        Vector3 direccionMirada =
            puntoMirada -
            transform.position;


        if (direccionMirada.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direccionMirada,
                    Vector3.up
                );
        }
    }


    // =========================================================
    // INPUT ROTACION
    // =========================================================

    private void LeerRotacion()
    {
        // =====================================================
        // TECLADO
        // =====================================================

        if (usarQE)
        {
            Keyboard teclado =
                Keyboard.current;


            if (teclado != null)
            {
                // Q
                if (teclado.qKey.wasPressedThisFrame)
                {
                    GirarIzquierda();
                }


                // E
                if (teclado.eKey.wasPressedThisFrame)
                {
                    GirarDerecha();
                }
            }
        }


        // =====================================================
        // MANDO
        // =====================================================

        if (usarBotonesMando)
        {
            Gamepad mando =
                Gamepad.current;


            if (mando != null)
            {
                // LB / L1
                if (mando.leftShoulder.wasPressedThisFrame)
                {
                    GirarIzquierda();
                }


                // RB / R1
                if (mando.rightShoulder.wasPressedThisFrame)
                {
                    GirarDerecha();
                }
            }
        }
    }


    // =========================================================
    // INPUT ZOOM
    // =========================================================

    private void LeerZoom()
    {
        // =====================================================
        // RUEDA DEL MOUSE
        // =====================================================

        if (usarRuedaMouse)
        {
            Mouse raton =
                Mouse.current;


            if (raton != null)
            {
                float rueda =
                    raton.scroll.ReadValue().y;


                if (Mathf.Abs(rueda) > 0.01f)
                {
                    // Rueda arriba = acercar
                    if (rueda > 0f)
                    {
                        AcercarZoom(
                            pasoZoomRueda
                        );
                    }

                    // Rueda abajo = alejar
                    else
                    {
                        AlejarZoom(
                            pasoZoomRueda
                        );
                    }
                }
            }
        }


        // =====================================================
        // D-PAD
        // =====================================================

        if (usarDPadZoom)
        {
            Gamepad mando =
                Gamepad.current;


            if (mando != null)
            {
                // D-Pad arriba = acercar
                if (mando.dpad.up.wasPressedThisFrame)
                {
                    AcercarZoom(
                        pasoZoomDPad
                    );
                }


                // D-Pad abajo = alejar
                if (mando.dpad.down.wasPressedThisFrame)
                {
                    AlejarZoom(
                        pasoZoomDPad
                    );
                }
            }
        }
    }


    // =========================================================
    // ZOOM
    // =========================================================

    public void AcercarZoom(float cantidad)
    {
        zoomObjetivo -= cantidad;


        zoomObjetivo =
            Mathf.Clamp(
                zoomObjetivo,
                zoomMinimo,
                zoomMaximo
            );
    }


    public void AlejarZoom(float cantidad)
    {
        zoomObjetivo += cantidad;


        zoomObjetivo =
            Mathf.Clamp(
                zoomObjetivo,
                zoomMinimo,
                zoomMaximo
            );
    }


    // =========================================================
    // ROTACION
    // =========================================================

    public void GirarDerecha()
    {
        anguloObjetivo +=
            gradosPorGiro;
    }


    public void GirarIzquierda()
    {
        anguloObjetivo -=
            gradosPorGiro;
    }
}