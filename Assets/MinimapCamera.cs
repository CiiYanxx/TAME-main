using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform target;
    public float height = 20f;
    public float smoothSpeed = 10f;

    [Header("Layer Settings")]
    public LayerMask minimapLayers;

    private bool firstSnapDone = false;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 15f;

            if (minimapLayers != 0)
                cam.cullingMask = minimapLayers;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition =
            new Vector3(
                target.position.x,
                target.position.y + height,
                target.position.z
            );

        // 🔥 First frame = instant snap
        if (!firstSnapDone)
        {
            transform.position = desiredPosition;
            firstSnapDone = true;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}