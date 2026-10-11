using UnityEngine;

/// <summary>
/// Hace que el enemigo persiga al jugador en linea recta.
/// El personaje debe tener el tag Player.
/// </summary>
public class EnemigoSeguirJugador : MonoBehaviour
{
    [SerializeField] Transform jugador;
    [SerializeField] float velocidad = 3.5f;
    [SerializeField] float distanciaDetenerse = 1.2f;
    [SerializeField] float velocidadRotacion = 10f;
    [SerializeField] bool bloquearEjeY = true;

    CharacterController controlador;

    void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (jugador == null)
        {
            GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
            if (encontrado != null)
                jugador = encontrado.transform;
        }
    }

    void Update()
    {
        if (jugador == null)
            return;

        Vector3 destino = jugador.position;
        Vector3 origen = transform.position;
        if (bloquearEjeY)
            destino.y = origen.y;

        Vector3 offset = destino - origen;
        float distancia = offset.magnitude;
        if (distancia <= distanciaDetenerse)
            return;

        Vector3 direccion = offset / distancia;
        Vector3 paso = direccion * velocidad * Time.deltaTime;

        if (controlador != null)
            controlador.Move(paso);
        else
            transform.position += paso;

        if (direccion.sqrMagnitude > 0.0001f)
        {
            Quaternion mirada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, mirada, velocidadRotacion * Time.deltaTime);
        }
    }

    public void AsignarJugador(Transform objetivo)
    {
        jugador = objetivo;
    }
}
