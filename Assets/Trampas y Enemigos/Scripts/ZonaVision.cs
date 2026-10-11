using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZonaVisionTotems : MonoBehaviour
{
    [SerializeField] private PuzzleSecuenciaLuces puzzle;

    private void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Player"))
        {
            if (puzzle != null)
            {
                // Le avisa al puzzle que el jugador esta lo suficientemente cerca para ver los totems
                puzzle.JugadorEntroEnZonaVision();
            }
        }
    }

    // Dibujar el area en el Editor para saber cuanto abarca
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}