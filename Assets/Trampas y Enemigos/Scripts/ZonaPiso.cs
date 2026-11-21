using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ZonaPiso : MonoBehaviour
{
    [SerializeField] private PuzzleSecuenciaLuces puzzle;
    [SerializeField] private GeneradorPlaca generadorPlaca;
    [SerializeField] private GeneradorEnemigos[] generadores = new GeneradorEnemigos[0];
    private GestorPisos gestor;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    public void Configurar(GestorPisos coordinador)
    {
        gestor = coordinador;
        if (puzzle != null) puzzle.ConfigurarZona(generadores);
        if (generadorPlaca != null) generadorPlaca.ControladoPorZona = true;
        foreach (var generador in generadores)
        {
            if (generador == null) continue;
            generador.ControladoPorZona = true;
            generador.FinalizarIntento();
        }
    }

    private void OnTriggerEnter(Collider otro)
    {
        VidaJugador jugador = otro.GetComponentInParent<VidaJugador>();
        if (jugador != null) RegistrarLlegada(jugador.transform);
    }

    public void RegistrarLlegada(Transform jugador)
    {
        if (gestor != null) gestor.Entrar(this, jugador);
    }

    public void IniciarIntento()
    {
        if (puzzle != null) puzzle.IniciarIntento();
        if (generadorPlaca != null) generadorPlaca.SpawnearPlaca();
        foreach (var generador in generadores)
            if (generador != null) generador.IniciarIntento();
    }

    public void FinalizarIntento()
    {
        if (puzzle != null) puzzle.FinalizarIntento();
        if (generadorPlaca != null) generadorPlaca.RetirarPlaca();
        foreach (var generador in generadores)
            if (generador != null) generador.FinalizarIntento();
    }
}
