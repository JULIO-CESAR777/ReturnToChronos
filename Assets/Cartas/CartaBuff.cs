using UnityEngine;

public enum TipoBuff
{
    DanioPorcentaje,
    VelocidadAtaquePorcentaje,
    VelocidadMovimientoPorcentaje,
    VidaMaxima
}

[System.Serializable]
public class EfectoBuff
{
    public TipoBuff tipo;

    [Tooltip("Ejemplo: 20 significa +20%. Para VidaMaxima significa +20 puntos.")]
    public float valor;
}

[CreateAssetMenu(
    fileName = "NuevaCartaBuff",
    menuName = "Buffs/Carta Buff"
)]
public class CartaBuff : ScriptableObject
{
    [Header("Informacion")]
    public string nombre;

    [TextArea(2, 5)]
    public string descripcion;

    public Sprite icono;

    [Header("Efectos")]
    public EfectoBuff[] efectos;
}