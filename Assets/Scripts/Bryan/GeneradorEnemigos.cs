using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instancia enemigos en puntos de aparicion, con limite y cadencia.
/// El puzzle puede detener la generacion al resolverse.
/// </summary>
public class GeneradorEnemigos : MonoBehaviour
{
    [SerializeField] GameObject prefabEnemigo;
    [SerializeField] Transform[] puntosAparicion;
    [SerializeField] Transform jugador;
    [SerializeField] int maximoVivos = 6;
    [SerializeField] float intervaloAparicion = 3f;
    [SerializeField] bool aparecerAlIniciar = true;
    [SerializeField] bool generarActivo = true;
    [Tooltip("Si el puzzle llama a DetenerGeneracion, tambien elimina a los enemigos que ya estan en escena.")]
    [SerializeField] bool eliminarVivosAlDetener;

    float temporizador;
    readonly List<GameObject> enemigosVivos = new List<GameObject>();

    public bool GenerarActivo => generarActivo;
    public int CantidadVivos => enemigosVivos.Count;

    void Start()
    {
        if (jugador == null)
        {
            GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
            if (encontrado != null)
                jugador = encontrado.transform;
        }

        if (aparecerAlIniciar && generarActivo)
            GenerarUno();
    }

    void Update()
    {
        if (!generarActivo)
            return;

        if (prefabEnemigo == null || puntosAparicion == null || puntosAparicion.Length == 0)
            return;

       // LimpiarLista();
       // if (enemigosVivos.Count >= maximoVivos)
          //  return;

        temporizador += Time.deltaTime;
        if (temporizador < intervaloAparicion)
            return;

        temporizador = 0f;
        GenerarUno();
    }

    public void GenerarUno()
    {
        if (!generarActivo)
            return;

        if (prefabEnemigo == null || puntosAparicion == null || puntosAparicion.Length == 0)
            return;

        //LimpiarLista();
       // if (enemigosVivos.Count >= maximoVivos)
          //  return;

        Transform punto = puntosAparicion[Random.Range(0, puntosAparicion.Length)];
        if (punto == null)
            return;

        GameObject enemigo = Instantiate(prefabEnemigo, punto.position, punto.rotation);
        enemigosVivos.Add(enemigo);

        EnemigoSeguirJugador seguimiento = enemigo.GetComponent<EnemigoSeguirJugador>();
        if (seguimiento != null && jugador != null)
            seguimiento.AsignarJugador(jugador);

        RastreadorEnemigoGenerado rastreador = enemigo.GetComponent<RastreadorEnemigoGenerado>();
        if (rastreador == null)
            rastreador = enemigo.AddComponent<RastreadorEnemigoGenerado>();
        rastreador.Vincular(this);
    }

    public void DetenerGeneracion()
    {
        generarActivo = false;
        temporizador = 0f;

        if (!eliminarVivosAlDetener)
            return;

        for (int i = enemigosVivos.Count - 1; i >= 0; i--)
        {
            if (enemigosVivos[i] != null)
                Destroy(enemigosVivos[i]);
        }

        enemigosVivos.Clear();
    }

    public void ReanudarGeneracion()
    {
        generarActivo = true;
    }

    public void AvisarEnemigoDestruido(GameObject enemigo)
    {
        enemigosVivos.Remove(enemigo);
    }

    void LimpiarLista()
    {
        enemigosVivos.RemoveAll(enemigo => enemigo == null);
    }
}

public class RastreadorEnemigoGenerado : MonoBehaviour
{
    GeneradorEnemigos duenio;

    public void Vincular(GeneradorEnemigos generador)
    {
        duenio = generador;
    }

    void OnDestroy()
    {
        if (duenio != null)
            duenio.AvisarEnemigoDestruido(gameObject);
    }
}
