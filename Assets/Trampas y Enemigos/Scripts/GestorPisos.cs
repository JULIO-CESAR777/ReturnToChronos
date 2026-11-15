using System;
using UnityEngine;

public class GestorPisos : MonoBehaviour
{
    [SerializeField] private Transform jugador;
    [Tooltip("Orden de los pisos, desde el inferior hasta el superior.")]
    [SerializeField] private ZonaPiso[] pisos;
    [Min(1)] [SerializeField] private int pisosQueRetrocedeAlMorir = 1;

    private int pisoActual = -1;
    private VidaJugador vida;

    private void Awake()
    {
        if (jugador == null || !jugador.TryGetComponent(out vida) ||
            pisos == null || pisos.Length == 0 || Array.Exists(pisos, piso => piso == null))
        {
            Debug.LogError("Configura el jugador y la lista ordenada de pisos.", this);
            enabled = false;
            return;
        }

        vida.Gestor = this;
        foreach (ZonaPiso piso in pisos) piso.Configurar(this);
    }

    private void Start()
    {
        if (pisoActual < 0) Entrar(pisos[0], jugador);
    }

    public void Entrar(ZonaPiso zona, Transform visitante)
    {
        if (visitante != jugador) return;
        int destino = Array.IndexOf(pisos, zona);
        if (destino < 0 || destino == pisoActual) return;

        if (pisoActual >= 0) pisos[pisoActual].FinalizarIntento();
        pisoActual = destino;
        zona.IniciarIntento();
    }

    public void Reaparecer()
    {
        if (pisoActual >= 0) pisos[pisoActual].FinalizarIntento();
        pisoActual = Mathf.Max(0, pisoActual - Mathf.Max(1, pisosQueRetrocedeAlMorir));
        ZonaPiso destino = pisos[pisoActual];
        Teletransportador.MoverJugador(jugador, destino.transform);
        vida.RestaurarTrasDerrota();
        destino.IniciarIntento();
    }
}
