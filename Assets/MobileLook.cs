using UnityEngine;

public class MobileLook : MonoBehaviour
{
    [Header("Settings")]
    public float Sensitivity = 0.2f;
    public float UpperLookLimit = 80f;
    public float LowerLookLimit = -80f;

    [Header("References")]
    public Transform PlayerBody; 

    private Vector2 touchStartPos;
    private Vector3 currentRotation;
    private int lookPointerId = -1;

    void Start()
    {
        currentRotation = transform.localEulerAngles;
        
        // If PlayerBody is not assigned, assume it's the parent
        if (PlayerBody == null) PlayerBody = transform.parent;
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // Only start "looking" if the touch is on the right half of the screen
            if (t.phase == TouchPhase.Began && t.position.x > Screen.width / 2)
            {
                lookPointerId = t.fingerId;
            }

            if (t.fingerId == lookPointerId)
            {
                if (t.phase == TouchPhase.Moved)
                {
                    // Rotate based on touch delta
                    float lookX = t.deltaPosition.x * Sensitivity;
                    float lookY = t.deltaPosition.y * Sensitivity;

                    // 1. Vertical Rotation (Camera only)
                    currentRotation.x -= lookY;
                    currentRotation.x = Mathf.Clamp(currentRotation.x, LowerLookLimit, UpperLookLimit);
                    transform.localRotation = Quaternion.Euler(currentRotation.x, 0, 0);

                    // 2. Horizontal Rotation (Player Body)
                    PlayerBody.Rotate(Vector3.up * lookX);
                }

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    lookPointerId = -1;
                }
            }
        }
    }
}