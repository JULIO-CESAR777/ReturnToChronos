using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlacaPresion : MonoBehaviour
{
    [Header("Pista Visual")]
    [SerializeField] private Light luzGuia;
    [SerializeField] private float intensidadEncendida = 1000f;

    private PuzzleSecuenciaLuces puzzle;
    private bool yaSeActivo = false;

    public void Inicializar(PuzzleSecuenciaLuces puzzleReferencia)
    {
        puzzle = puzzleReferencia;
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (yaSeActivo) return;

        if (otro.CompareTag("Player"))
        {
            yaSeActivo = true;

            // Cambia el color o apaga la luz guía al activarse
            if (luzGuia != null)
            {
                luzGuia.color = Color.green;
                luzGuia.intensity = intensidadEncendida;
            }

            if (puzzle != null)
            {
                puzzle.ActivarDesdePlaca();
            }

            Debug.Log("¡Placa de presión activada!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}