
using UnityEngine;
using System.Collections;

public class VidaEnemigo : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 100;

    [Header("Efectos de muerte")]
    [SerializeField] private ParticleSystem particulasMuerte;
    [SerializeField] private Transform puntoParticulas;

    [Header("Duracion de particulas")]
    [SerializeField] private float tiempoEmision = 0.5f;
    [SerializeField] private float tiempoMaximoEfecto = 5f;

    private int vidaActual;
    private bool estaMuerto = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(int cantidad)
    {
        if (estaMuerto || cantidad <= 0)
            return;

        vidaActual = Mathf.Max(0, vidaActual - cantidad);

        Debug.Log(
            gameObject.name +
            " recibió " +
            cantidad +
            " de daño. Vida restante: " +
            vidaActual
        );

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        if (estaMuerto)
            return;

        estaMuerto = true;

        Debug.Log(gameObject.name + " murió.");

        if (particulasMuerte != null)
        {
            Vector3 posicion = puntoParticulas != null
                ? puntoParticulas.position
                : transform.position;

            ParticleSystem efecto = Instantiate(
                particulasMuerte,
                posicion,
                Quaternion.identity
            );

            efecto.gameObject.SetActive(true);
            ParticleSystem[] sistemas =
                efecto.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem sistema in sistemas)
            {
                var main = sistema.main;
                main.loop = false;
                main.stopAction = ParticleSystemStopAction.None;
            }

            efecto.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            efecto.Play(true);
            ControladorParticulasMuerte controlador =
                efecto.gameObject.AddComponent<ControladorParticulasMuerte>();

            controlador.Inicializar(
                tiempoEmision,
                tiempoMaximoEfecto
            );
        }

        Destroy(gameObject);
    }
}

public class ControladorParticulasMuerte : MonoBehaviour
{
    private float tiempoEmision;
    private float tiempoMaximo;

    private ParticleSystem[] sistemas;

    public void Inicializar(float emision, float maximo)
    {
        tiempoEmision = Mathf.Max(0f, emision);
        tiempoMaximo = Mathf.Max(0.1f, maximo);

        sistemas = GetComponentsInChildren<ParticleSystem>(true);

        StartCoroutine(ControlarEfecto());
    }

    private IEnumerator ControlarEfecto()
    {
        // Permitir que se reproduzca el efecto
        yield return new WaitForSeconds(tiempoEmision);
        foreach (ParticleSystem sistema in sistemas)
        {
            if (sistema != null)
            {
                sistema.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );
            }
        }

        // Esperar a que desaparezcan naturalmente
        float tiempoRestante = Mathf.Max(
            0f,
            tiempoMaximo - tiempoEmision
        );

        float transcurrido = 0f;

        while (transcurrido < tiempoRestante)
        {
            bool hayParticulas = false;

            foreach (ParticleSystem sistema in sistemas)
            {
                if (sistema != null && sistema.IsAlive(true))
                {
                    hayParticulas = true;
                    break;
                }
            }

            if (!hayParticulas)
                break;

            transcurrido += Time.deltaTime;
            yield return null;
        }

        // Destruccion garantizada
        Destroy(gameObject);
    }
}
