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

        transform.rotation =
            Quaternion.LookRotation(-camara.forward, camara.up);
    }
}