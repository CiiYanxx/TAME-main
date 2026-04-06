using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotateTouch : MonoBehaviour, IDragHandler
{
    [Header("Target Character")]
    [Tooltip("I-drag dito ang PLAYER object mula sa Hierarchy")]
    public Transform characterTransform;

    [Header("Rotation Settings")]
    [Range(0.1f, 2.0f)]
    public float sensitivity = 0.8f; // Sensitivity ng drag
    public float smoothing = 15f;    // Bilis ng paghabol ng rotation (Smoothness)

    private float targetYRotation;
    private float currentYRotation;

    void Start()
    {
        if (characterTransform != null)
        {
            // Kunin ang kasalukuyang rotation ng character sa simula
            targetYRotation = characterTransform.eulerAngles.y;
            currentYRotation = targetYRotation;
        }
        else
        {
            Debug.LogWarning("Pakilagay ang Character Transform sa Inspector!");
        }
    }

    // Tinatawag ng Unity UI system kapag dinadrag ang Image
    public void OnDrag(PointerEventData eventData)
    {
        if (characterTransform != null)
        {
            // Binabawasan ang target rotation base sa galaw ng mouse/touch (X-axis)
            targetYRotation -= eventData.delta.x * sensitivity;
        }
    }

    // Ginagamit ang LateUpdate para maiwasan ang panginginig (jitter)
    // dahil sinisiguro nito na tapos na ang lahat ng physics bago i-update ang view
    void LateUpdate()
    {
        if (characterTransform != null)
        {
            // LerpAngle ay ginagamit para sa smooth transition kahit lumampas ng 360 degrees
            currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, Time.deltaTime * smoothing);
            
            // I-apply ang rotation (Y-axis lang para hindi tumagilid ang character)
            characterTransform.rotation = Quaternion.Euler(0, currentYRotation, 0);
        }
    }
}