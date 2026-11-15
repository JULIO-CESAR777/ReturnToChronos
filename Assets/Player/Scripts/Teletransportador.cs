using System;
using UnityEngine;

public class Teletransportador : MonoBehaviour
{
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform destino;


    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Teletransportar();


        }
    
        
    }

    public void Teletransportar()
    {
        if (jugador == null || destino == null)
            return;

        MoverJugador(jugador, destino);
        if (destino.TryGetComponent(out ZonaPiso zona))
            zona.RegistrarLlegada(jugador);
    }

    public static void MoverJugador(Transform jugador, Transform destino)
    {
        CharacterController controller =
            jugador.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        jugador.position = destino.position;
        jugador.rotation = destino.rotation;

        if (controller != null)
            controller.enabled = true;
    }
}