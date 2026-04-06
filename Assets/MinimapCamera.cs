using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform target;         // Player
    public float height = 20f;       // Taas ng camera
    public float smoothSpeed = 10f;  // Bilis ng pagsunod

    [Header("Layer Settings")]
    public LayerMask minimapLayers;  // Dito mo i-check ang "MinimapIcon" at "Ground"

    void Start()
    {
        // Siguraduhin na ang camera ay naka-Orthographic para mukhang totoong map
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 15f; // Adjust mo 'to para sa zoom level ng map
            if(minimapLayers != 0) cam.cullingMask = minimapLayers;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Position follow (walang rotation para laging North ang map)
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y + height, target.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Look straight down
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}