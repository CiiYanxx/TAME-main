using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    public RectTransform container;      // Yung invisible area (JoystickArea)
    public RectTransform background;     // Yung 'Center'
    public RectTransform handle;         // Yung 'Stick'

    [Header("Settings")]
    public float joystickRadius = 100f;
    public float returnSmoothTime = 0.1f; // Speed ng pagbalik sa dating pwesto
    
    [Header("Visual Effects")]
    public float pressScale = 1.15f;
    public Color normalColor = Color.white;
    public Color pressColor = new Color(0.35f, 0.85f, 1f, 1f); 

    private Vector2 inputVector = Vector2.zero;
    private Vector2 defaultPosition; // Original position ng 'Center'
    private Vector2 currentVelocity;
    private bool isPressed = false;
    private Image backImage, handleImage;
    private Coroutine scaleRoutine;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public Vector2 Direction => inputVector;

    void Awake()
    {
        // I-save ang original position para doon babalik pag binitawan
        if (background)
        {
            defaultPosition = background.anchoredPosition;
            backImage = background.GetComponent<Image>();
        }
        if (handle) handleImage = handle.GetComponent<Image>();
    }

    void Update()
    {
        // Smoothly ibalik ang 'Center' sa defaultPosition at ang 'Stick' sa zero
        if (!isPressed && background)
        {
            background.anchoredPosition = Vector2.SmoothDamp(background.anchoredPosition, defaultPosition, ref currentVelocity, returnSmoothTime);
            handle.anchoredPosition = Vector2.SmoothDamp(handle.anchoredPosition, Vector2.zero, ref currentVelocity, returnSmoothTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        // Lipat ang background sa touch point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        background.anchoredPosition = localPoint;
        handle.anchoredPosition = Vector2.zero;

        ApplyVisuals(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Gamitin ang local point relative sa background para pantay ang movement
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Siguraduhin na ang input ay normalized base sa radius
            inputVector = localPoint / joystickRadius;

            // Clamp: Wag palabasin sa radius
            if (inputVector.magnitude > 1.0f)
                inputVector = inputVector.normalized;

            // Sakto ang posisyon ng stick sa touch direction
            handle.anchoredPosition = inputVector * joystickRadius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        inputVector = Vector2.zero; // Itigil ang movement sa player controller
        
        ApplyVisuals(false);
    }

    private void ApplyVisuals(bool pressing)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleRoutine(pressing));

        Color targetCol = pressing ? pressColor : normalColor;
        if (backImage) backImage.CrossFadeColor(targetCol, 0.1f, true, true);
        if (handleImage) handleImage.CrossFadeColor(targetCol, 0.1f, true, true);
    }

    IEnumerator ScaleRoutine(bool enlarge)
    {
        Vector3 targetScale = enlarge ? Vector3.one * pressScale : Vector3.one;
        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            background.localScale = Vector3.Lerp(background.localScale, targetScale, t / 0.1f);
            yield return null;
        }
        background.localScale = targetScale;
    }
}