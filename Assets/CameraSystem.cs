using UnityEngine;
using System.Collections;

public class CameraSystem : MonoBehaviour
{
    public static CameraSystem Instance { get; private set; }

    [Header("Camera View Settings")]
    public Transform conversationCameraAnchor; 
    
    [Tooltip("Bilis ng pag-zoom papunta kay NPC")]
    public float zoomInSpeed = 5f;

    [Tooltip("Bilis ng pag-zoom pabalik sa Player")]
    public float zoomOutSpeed = 2f; // Karaniwang mas mabagal para sa cinematic feel

    [Header("HUD Elements to Hide")]
    public GameObject[] hudElements; 

    private Transform mainCamTransform;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool isZoomed = false;
    private bool isReturning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (Camera.main != null) mainCamTransform = Camera.main.transform;
    }

    public void EnableConversationMode(bool state)
    {
        if (state)
        {
            if (!isZoomed)
            {
                originalPos = mainCamTransform.position;
                originalRot = mainCamTransform.rotation;
            }
            isZoomed = true;
            isReturning = false;
            ToggleHUD(false);
        }
        else
        {
            isZoomed = false;
            isReturning = true;
            ToggleHUD(true);
        }
    }

    private void ToggleHUD(bool state)
    {
        if (hudElements == null) return;
        foreach (GameObject element in hudElements)
        {
            if (element != null) element.SetActive(state);
        }
    }

    void LateUpdate()
    {
        if (mainCamTransform == null || conversationCameraAnchor == null) return;

        if (isZoomed)
        {
            // Gamit ang zoomInSpeed
            mainCamTransform.position = Vector3.Lerp(mainCamTransform.position, conversationCameraAnchor.position, Time.deltaTime * zoomInSpeed);
            mainCamTransform.rotation = Quaternion.Slerp(mainCamTransform.rotation, conversationCameraAnchor.rotation, Time.deltaTime * zoomInSpeed);
        }
        else if (isReturning)
        {
            // Gamit ang zoomOutSpeed para sa pagbalik
            mainCamTransform.position = Vector3.Lerp(mainCamTransform.position, originalPos, Time.deltaTime * zoomOutSpeed);
            mainCamTransform.rotation = Quaternion.Slerp(mainCamTransform.rotation, originalRot, Time.deltaTime * zoomOutSpeed);
            
            // I-check kung malapit na para i-stop ang lerp
            if (Vector3.Distance(mainCamTransform.position, originalPos) < 0.05f)
            {
                mainCamTransform.position = originalPos;
                mainCamTransform.rotation = originalRot;
                isReturning = false; 
            }
        }
    }
}