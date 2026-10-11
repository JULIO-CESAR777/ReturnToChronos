using UnityEngine;

public class GeneradorPlaca : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject prefabPlaca;
    [SerializeField] private PuzzleSecuenciaLuces puzzle;

    [Header("Puntos de Spawn en el Mapa")]
    [SerializeField] private Transform[] puntosPosibles;

    private void Start()
    {
        SpawnearPlaca();
    }

    public void SpawnearPlaca()
    {
        if (puntosPosibles == null || puntosPosibles.Length == 0 || prefabPlaca == null)
        {
            Debug.LogWarning("GeneradorPlaca: Faltan referencias o puntos de spawn.", this);
            return;
        }

        // Elige un punto aleatorio del arreglo
        int indiceAleatorio = Random.Range(0, puntosPosibles.Length);
        Transform puntoElegido = puntosPosibles[indiceAleatorio];

        // Instancia la placa en esa posicion
        GameObject nuevaPlaca = Instantiate(prefabPlaca, puntoElegido.position, puntoElegido.rotation);

        // Pasa la referencia del puzzle a la placa instanciada
        PlacaPresion scriptPlaca = nuevaPlaca.GetComponent<PlacaPresion>();
        if (scriptPlaca != null)
        {
            scriptPlaca.Inicializar(puzzle);
        }
    }
}