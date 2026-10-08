using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ControladorJugador : MonoBehaviour
{
    // =========================================================
    // MOVIMIENTO
    // =========================================================

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;

    [SerializeField] private float aceleracionSuelo = 35f;

    [SerializeField] private float aceleracionAire = 10f;

    [SerializeField] private float gravedad = -20f;

    [SerializeField] private Transform transformCamara;

    [SerializeField] private bool movimientoRelativoACamara = true;

    [SerializeField] private float zonaMuertaMovimiento = 0.15f;


    // =========================================================
    // SALTO / BUNNY HOP
    // =========================================================

    [Header("Salto / Bunny Hop")]

    [Tooltip("Altura aproximada del salto.")]
    [SerializeField] private float alturaSalto = 1.5f;

    [Tooltip("Si esta activo, mantener el boton de salto hace bunny hop automaticamente.")]
    [SerializeField] private bool bunnyHopAutomatico = true;

    [Tooltip("Multiplicador de velocidad cada vez que haces un bunny hop.")]
    [SerializeField] private float multiplicadorBunnyHop = 1.08f;

    [Tooltip("Velocidad horizontal maxima alcanzable haciendo bunny hop.")]
    [SerializeField] private float velocidadMaximaBunnyHop = 10f;

    [Tooltip("Permite saltar un instante despues de abandonar una plataforma.")]
    [SerializeField] private float tiempoCoyote = 0.12f;

    [Tooltip("Recuerda el input de salto unos instantes antes de tocar el piso.")]
    [SerializeField] private float bufferSalto = 0.12f;
   
    
    
    [Header("Knockback")]
    [SerializeField] private float frenadoKnockback = 20f;

    // =========================================================
    // APUNTADO
    // =========================================================

    [Header("Apuntado")]
    [SerializeField] private bool rotarHaciaDireccion = true;

    [SerializeField] private float velocidadRotacion = 18f;

    [SerializeField] private float zonaMuertaApuntado = 0.25f;

    [SerializeField] private bool usarRaton = true;


    // =========================================================
    // VARIABLES INTERNAS
    // =========================================================

    private CharacterController controlador;

    private float velocidadVertical;

    private Vector3 velocidadHorizontal;
    
    private Vector3 velocidadKnockback;

    private Vector3 direccionMovimiento;

    private Vector3 direccionMirada = Vector3.forward;

    private float ultimoMomentoEnSuelo = -999f;

    private float ultimaSolicitudSalto = -999f;


    // =========================================================
    // VARIABLES PUBLICAS
    // =========================================================

    public Vector3 DireccionMovimiento => direccionMovimiento;

    public Vector3 DireccionMirada => direccionMirada;

    public bool EstaMoviendose =>
        direccionMovimiento.sqrMagnitude > 0.01f;

    public bool EstaApuntando { get; private set; }

    public bool EstaEnSuelo =>
        controlador != null && controlador.isGrounded;

    public bool EstaSaltando =>
        !EstaEnSuelo && velocidadVertical > 0f;

    public float VelocidadActual =>
        velocidadHorizontal.magnitude;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        controlador =
            GetComponent<CharacterController>();

        if (transformCamara == null &&
            Camera.main != null)
        {
            transformCamara =
                Camera.main.transform;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        ObtenerEjesCamara(
            out Vector3 adelante,
            out Vector3 derecha
        );

        RegistrarSalto();

        MoverJugador(
            adelante,
            derecha
        );

        ActualizarDireccion(
            adelante,
            derecha
        );

        RotarJugador();
    }


    // =========================================================
    // MOVIMIENTO
    // =========================================================

   private void MoverJugador(
    Vector3 adelante,
    Vector3 derecha
)
{
    Vector2 entrada =
        LeerMovimiento();


    // -----------------------------------------------------
    // DIRECCION DE INPUT
    // -----------------------------------------------------

    direccionMovimiento =
        adelante * entrada.y +
        derecha * entrada.x;


    if (direccionMovimiento.sqrMagnitude > 1f)
    {
        direccionMovimiento.Normalize();
    }


    // -----------------------------------------------------
    // DETECTAR SUELO
    // -----------------------------------------------------

    bool enSuelo =
        controlador.isGrounded;


    if (enSuelo)
    {
        ultimoMomentoEnSuelo =
            Time.time;


        if (velocidadVertical < 0f)
        {
            velocidadVertical = -2f;
        }
    }


    // -----------------------------------------------------
    // ¿PODEMOS SALTAR?
    // -----------------------------------------------------

    bool dentroTiempoCoyote =
        Time.time - ultimoMomentoEnSuelo
        <= tiempoCoyote;


    bool saltoEnBuffer =
        Time.time - ultimaSolicitudSalto
        <= bufferSalto;


    bool puedeSaltar =
        dentroTiempoCoyote &&
        saltoEnBuffer;


    // =====================================================
    // SALTO
    // =====================================================

    if (puedeSaltar)
    {
        EjecutarSalto();

        // Consumimos el salto
        ultimaSolicitudSalto = -999f;

        // Evita doble salto por el coyote time
        ultimoMomentoEnSuelo = -999f;
    }
    else
    {
        // =================================================
        // MOVIMIENTO EN SUELO
        // =================================================

        if (enSuelo)
        {
            MoverEnSuelo();
        }

        // =================================================
        // MOVIMIENTO EN AIRE
        // =================================================

        else
        {
            MoverEnAire();
        }
    }


    // -----------------------------------------------------
    // GRAVEDAD
    // -----------------------------------------------------

    velocidadVertical +=
        gravedad * Time.deltaTime;


    // -----------------------------------------------------
    // MOVIMIENTO FINAL
    // -----------------------------------------------------

    // Movimiento normal + bunnyhop + knockback
    Vector3 movimientoFinal =
        velocidadHorizontal +
        velocidadKnockback;


    movimientoFinal.y =
        velocidadVertical;


    // -----------------------------------------------------
    // MOVER CHARACTER CONTROLLER
    // -----------------------------------------------------

    controlador.Move(
        movimientoFinal *
        Time.deltaTime
    );


    // -----------------------------------------------------
    // FRENAR KNOCKBACK
    // -----------------------------------------------------

    velocidadKnockback =
        Vector3.MoveTowards(
            velocidadKnockback,
            Vector3.zero,
            frenadoKnockback *
            Time.deltaTime
        );
}


    // =========================================================
    // MOVIMIENTO EN SUELO
    // =========================================================

    private void MoverEnSuelo()
    {
        Vector3 velocidadObjetivo =
            direccionMovimiento *
            velocidadMovimiento;


        velocidadHorizontal =
            Vector3.MoveTowards(
                velocidadHorizontal,
                velocidadObjetivo,
                aceleracionSuelo *
                Time.deltaTime
            );
    }


    // =========================================================
    // MOVIMIENTO EN AIRE
    // =========================================================

    private void MoverEnAire()
    {
        if (direccionMovimiento.sqrMagnitude < 0.01f)
            return;


        Vector3 direccionDeseada =
            direccionMovimiento.normalized;


        // Cuanta velocidad ya tenemos
        // en la direccion deseada
        float velocidadEnDireccion =
            Vector3.Dot(
                velocidadHorizontal,
                direccionDeseada
            );


        // Permitimos acelerar hasta la velocidad base
        // en esa direccion.
        float velocidadFaltante =
            velocidadMovimiento -
            velocidadEnDireccion;


        if (velocidadFaltante <= 0f)
            return;


        float aceleracion =
            aceleracionAire *
            Time.deltaTime;


        aceleracion =
            Mathf.Min(
                aceleracion,
                velocidadFaltante
            );


        velocidadHorizontal +=
            direccionDeseada *
            aceleracion;
    }


    // =========================================================
    // SALTO
    // =========================================================

    private void EjecutarSalto()
    {
        // Formula para alcanzar una altura determinada.
        velocidadVertical =
            Mathf.Sqrt(
                alturaSalto *
                -2f *
                gravedad
            );


        AplicarBunnyHop();
    }


    // =========================================================
    // BUNNY HOP
    // =========================================================

    private void AplicarBunnyHop()
    {
        float velocidadActual =
            velocidadHorizontal.magnitude;


        // Si estamos empezando a movernos,
        // damos la velocidad base.
        if (velocidadActual <
            velocidadMovimiento)
        {
            if (direccionMovimiento.sqrMagnitude >
                0.01f)
            {
                velocidadHorizontal =
                    direccionMovimiento.normalized *
                    velocidadMovimiento;
            }

            return;
        }


        // Ganamos velocidad al encadenar saltos.
        float nuevaVelocidad =
            velocidadActual *
            multiplicadorBunnyHop;


        nuevaVelocidad =
            Mathf.Min(
                nuevaVelocidad,
                velocidadMaximaBunnyHop
            );


        if (velocidadHorizontal.sqrMagnitude >
            0.01f)
        {
            velocidadHorizontal =
                velocidadHorizontal.normalized *
                nuevaVelocidad;
        }
    }


    // =========================================================
    // REGISTRAR INPUT SALTO
    // =========================================================

    private void RegistrarSalto()
    {
        bool presionado =
            SePulsoSalto();


        bool mantenido =
            SeMantieneSalto();


        // Salto normal.
        if (presionado)
        {
            ultimaSolicitudSalto =
                Time.time;
        }


        // Bunny hop automático.
        if (bunnyHopAutomatico &&
            mantenido)
        {
            ultimaSolicitudSalto =
                Time.time;
        }
    }


    // =========================================================
    // DIRECCION / APUNTADO
    // =========================================================

    private void ActualizarDireccion(
        Vector3 adelante,
        Vector3 derecha
    )
    {
        Vector2 stick =
            LeerApuntadoStick();


        // -----------------------------------------------------
        // STICK DERECHO
        // -----------------------------------------------------

        if (stick.sqrMagnitude >=
            zonaMuertaApuntado *
            zonaMuertaApuntado)
        {
            direccionMirada =
                (
                    adelante * stick.y +
                    derecha * stick.x
                ).normalized;


            EstaApuntando = true;

            return;
        }


        // -----------------------------------------------------
        // RATON
        // -----------------------------------------------------

        if (usarRaton &&
            IntentarApuntarConRaton(
                out Vector3 direccionRaton))
        {
            direccionMirada =
                direccionRaton;


            EstaApuntando = true;

            return;
        }


        EstaApuntando = false;


        // Si no estamos apuntando,
        // miramos hacia donde caminamos.
        if (direccionMovimiento.sqrMagnitude >
            0.01f)
        {
            direccionMirada =
                direccionMovimiento.normalized;
        }
    }


    // =========================================================
    // ROTACION
    // =========================================================

    private void RotarJugador()
    {
        if (!rotarHaciaDireccion)
            return;


        if (direccionMirada.sqrMagnitude <
            0.01f)
        {
            return;
        }


        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(
                direccionMirada
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidadRotacion *
                Time.deltaTime
            );
    }


    // =========================================================
    // CAMARA
    // =========================================================

    private void ObtenerEjesCamara(
        out Vector3 adelante,
        out Vector3 derecha
    )
    {
        if (movimientoRelativoACamara &&
            transformCamara != null)
        {
            adelante =
                transformCamara.forward;

            derecha =
                transformCamara.right;
        }
        else
        {
            adelante =
                Vector3.forward;

            derecha =
                Vector3.right;
        }


        adelante.y = 0f;

        derecha.y = 0f;


        adelante.Normalize();

        derecha.Normalize();
    }


    // =========================================================
    // INPUT MOVIMIENTO
    // =========================================================

    private Vector2 LeerMovimiento()
    {
        Vector2 entrada =
            Vector2.zero;


        Gamepad mando =
            Gamepad.current;


        if (mando != null)
        {
            entrada +=
                mando.leftStick.ReadValue();
        }


        Keyboard teclado =
            Keyboard.current;


        if (teclado != null)
        {
            if (teclado.wKey.isPressed ||
                teclado.upArrowKey.isPressed)
            {
                entrada.y += 1f;
            }


            if (teclado.sKey.isPressed ||
                teclado.downArrowKey.isPressed)
            {
                entrada.y -= 1f;
            }


            if (teclado.dKey.isPressed ||
                teclado.rightArrowKey.isPressed)
            {
                entrada.x += 1f;
            }


            if (teclado.aKey.isPressed ||
                teclado.leftArrowKey.isPressed)
            {
                entrada.x -= 1f;
            }
        }


        if (entrada.magnitude <
            zonaMuertaMovimiento)
        {
            return Vector2.zero;
        }


        return Vector2.ClampMagnitude(
            entrada,
            1f
        );
    }


    // =========================================================
    // INPUT SALTO
    // =========================================================

    private bool SePulsoSalto()
    {
        Keyboard teclado =
            Keyboard.current;


        if (teclado != null &&
            teclado.spaceKey.wasPressedThisFrame)
        {
            return true;
        }


        Gamepad mando =
            Gamepad.current;


        if (mando != null &&
            mando.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }


        return false;
    }


    private bool SeMantieneSalto()
    {
        Keyboard teclado =
            Keyboard.current;


        if (teclado != null &&
            teclado.spaceKey.isPressed)
        {
            return true;
        }


        Gamepad mando =
            Gamepad.current;


        if (mando != null &&
            mando.buttonSouth.isPressed)
        {
            return true;
        }


        return false;
    }


    // =========================================================
    // INPUT APUNTADO
    // =========================================================

    private Vector2 LeerApuntadoStick()
    {
        Gamepad mando =
            Gamepad.current;


        if (mando == null)
            return Vector2.zero;


        return
            mando.rightStick.ReadValue();
    }


    // =========================================================
    // APUNTADO RATON
    // =========================================================

    private bool IntentarApuntarConRaton(
        out Vector3 direccion)
    {
        direccion =
            Vector3.zero;


        Mouse raton =
            Mouse.current;


        if (raton == null ||
            transformCamara == null)
        {
            return false;
        }


        Camera camara =
            transformCamara.GetComponent<Camera>();


        if (camara == null)
        {
            camara =
                Camera.main;
        }


        if (camara == null)
            return false;


        Ray rayo =
            camara.ScreenPointToRay(
                raton.position.ReadValue()
            );


        Plane suelo =
            new Plane(
                Vector3.up,
                transform.position
            );


        if (!suelo.Raycast(
                rayo,
                out float distancia))
        {
            return false;
        }


        Vector3 punto =
            rayo.GetPoint(
                distancia
            );


        Vector3 diferencia =
            punto -
            transform.position;


        diferencia.y = 0f;


        if (diferencia.sqrMagnitude <
            0.05f)
        {
            return false;
        }


        direccion =
            diferencia.normalized;


        return true;
    }


    // =========================================================
    // ATAQUE
    // =========================================================

    public static bool SePulsoAtaque()
    {
        Mouse raton =
            Mouse.current;


        if (raton != null &&
            raton.leftButton.wasPressedThisFrame)
        {
            return true;
        }


        Gamepad mando =
            Gamepad.current;


        if (mando != null &&
            mando.rightTrigger.wasPressedThisFrame)
        {
            return true;
        }


        return false;
    }
    
    public void AplicarKnockback(
        Vector3 direccion,
        float fuerza
    )
    {
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f)
            return;

        velocidadKnockback =
            direccion.normalized *
            fuerza;
    }
}