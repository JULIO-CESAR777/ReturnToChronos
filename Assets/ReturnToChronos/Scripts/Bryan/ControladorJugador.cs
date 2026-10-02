using UnityEngine;
using UnityEngine.InputSystem;

/// Dual stick 2.5D: stick izquierdo mueve, stick derecho apunta (direccion de ataque).
/// Teclado/raton: WASD mueve y el raton apunta al suelo.
[RequireComponent(typeof(CharacterController))]
public class ControladorJugador : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidadMovimiento = 5f;
    [SerializeField] float gravedad = -20f;
    [SerializeField] Transform transformCamara;
    [SerializeField] bool movimientoRelativoACamara = true;
    [SerializeField] float zonaMuertaMovimiento = 0.15f;

    [Header("Apuntado")]
    [Tooltip("El personaje mira hacia el stick derecho / raton, no hacia donde camina.")]
    [SerializeField] bool rotarHaciaApuntado = true;
    [SerializeField] float velocidadRotacion = 18f;
    [SerializeField] float zonaMuertaApuntado = 0.25f;
    [SerializeField] bool usarRatonSiNoHayStick = true;
    [SerializeField] Transform indicadorApuntado;

    CharacterController controlador;
    float velocidadVertical;
    Vector3 direccionApuntado = Vector3.forward;

    public Vector3 DireccionApuntado => direccionApuntado;
    public bool EstaApuntando { get; private set; }

    void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (transformCamara == null && Camera.main != null)
            transformCamara = Camera.main.transform;
    }

    void Update()
    {
        ObtenerEjesCamara(out Vector3 adelante, out Vector3 derecha);

        Vector2 entradaMovimiento = LeerMovimiento();
        Vector3 desplazamientoPlano = (adelante * entradaMovimiento.y + derecha * entradaMovimiento.x) * velocidadMovimiento;

        if (controlador.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -2f;

        velocidadVertical += gravedad * Time.deltaTime;
        Vector3 desplazamiento = desplazamientoPlano;
        desplazamiento.y = velocidadVertical;
        controlador.Move(desplazamiento * Time.deltaTime);

        ActualizarApuntado(adelante, derecha);

        if (rotarHaciaApuntado && direccionApuntado.sqrMagnitude > 0.01f)
        {
            Quaternion mirada = Quaternion.LookRotation(direccionApuntado);
            transform.rotation = Quaternion.Slerp(transform.rotation, mirada, velocidadRotacion * Time.deltaTime);
        }

        ActualizarIndicador();
    }

    void ObtenerEjesCamara(out Vector3 adelante, out Vector3 derecha)
    {
        if (movimientoRelativoACamara && transformCamara != null)
        {
            adelante = transformCamara.forward;
            derecha = transformCamara.right;
        }
        else
        {
            adelante = Vector3.forward;
            derecha = Vector3.right;
        }

        adelante.y = 0f;
        derecha.y = 0f;
        adelante.Normalize();
        derecha.Normalize();
    }

    void ActualizarApuntado(Vector3 adelante, Vector3 derecha)
    {
        Vector2 stickDerecho = LeerApuntadoStick();
        if (stickDerecho.sqrMagnitude >= zonaMuertaApuntado * zonaMuertaApuntado)
        {
            direccionApuntado = (adelante * stickDerecho.y + derecha * stickDerecho.x).normalized;
            EstaApuntando = true;
            return;
        }

        if (usarRatonSiNoHayStick && IntentarApuntarConRaton(out Vector3 apuntadoRaton))
        {
            direccionApuntado = apuntadoRaton;
            EstaApuntando = true;
            return;
        }

        EstaApuntando = false;
    }

    bool IntentarApuntarConRaton(out Vector3 direccion)
    {
        direccion = Vector3.zero;
        Mouse raton = Mouse.current;
        Camera camara = transformCamara != null ? transformCamara.GetComponent<Camera>() : Camera.main;
        if (raton == null || camara == null)
            return false;

        Ray rayo = camara.ScreenPointToRay(raton.position.ReadValue());
        Plane suelo = new Plane(Vector3.up, transform.position);
        if (!suelo.Raycast(rayo, out float distancia))
            return false;

        Vector3 punto = rayo.GetPoint(distancia);
        Vector3 offset = punto - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.05f)
            return false;

        direccion = offset.normalized;
        return true;
    }

    void ActualizarIndicador()
    {
        if (indicadorApuntado == null)
            return;

        indicadorApuntado.position = transform.position + direccionApuntado * 1.5f + Vector3.up * 0.1f;
        indicadorApuntado.rotation = Quaternion.LookRotation(direccionApuntado);
    }

    Vector2 LeerMovimiento()
    {
        Vector2 valor = Vector2.zero;

        Gamepad mando = Gamepad.current;
        if (mando != null)
            valor += mando.leftStick.ReadValue();

        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) valor.y += 1f;
            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) valor.y -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) valor.x += 1f;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) valor.x -= 1f;
        }

        if (valor.magnitude < zonaMuertaMovimiento)
            return Vector2.zero;

        return Vector2.ClampMagnitude(valor, 1f);
    }

    Vector2 LeerApuntadoStick()
    {
        Gamepad mando = Gamepad.current;
        if (mando == null)
            return Vector2.zero;

        return mando.rightStick.ReadValue();
    }

    /// Disparo / golpe: RT, R2 o clic izquierdo.
 
    public static bool SePulsoAtaque()
    {
        Mouse raton = Mouse.current;
        if (raton != null && raton.leftButton.wasPressedThisFrame)
            return true;

        Gamepad mando = Gamepad.current;
        return mando != null && (mando.rightTrigger.wasPressedThisFrame || mando.buttonWest.wasPressedThisFrame);
    }
}
