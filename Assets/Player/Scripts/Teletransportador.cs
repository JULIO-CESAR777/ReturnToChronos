using System;
using UnityEngine;

public class Teletransportador : MonoBehaviour
{
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform destino;


    void OnTriggerEnter(Collider other)
    {
        Teletransportar();
        
    }

    public void Teletransportar()
    {
        if (jugador == null || destino == null)
            return;

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