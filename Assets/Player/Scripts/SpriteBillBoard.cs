using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] private Transform camara;

    void Start()
    {
        if (camara == null && Camera.main != null)
            camara = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camara == null)
            return;
        Vector3 direccionCamara = camara.forward;
        direccionCamara.y = 0f;

        if (direccionCamara.sqrMagnitude > 0.001f)
        {
            // sprite hacia la camara manteniendolo vertical al suelo
            transform.rotation = Quaternion.LookRotation(-direccionCamara, Vector3.up);
        }
    }
}