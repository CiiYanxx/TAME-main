using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    public float Radius = 100f;             // max distance in pixels
    public float ReturnSmoothTime = 0.08f;  // Smooth return time
    public float PressScale = 1.15f;
    public Color NormalColor = Color.white;
    public Color PressColor = Color.white;
    public float ScaleDuration = 0.12f;

    [Header("References")]
    public RectTransform StickRect;         // inner handle
    public RectTransform BackgroundRect;    // background area (center reference)
    Canvas m_Canvas;
    GraphicRaycaster m_Raycaster;
    Image backImage, stickImage;

    Vector2 defaultAnchoredPos;
    Vector2 pointerOffset;
    bool isPressed = false;
    int pointerId = -1;
    Vector2 currentVelocity; // for SmoothDamp (unused but kept for future)
    Coroutine scaleRoutine;

    /// <summary>
    /// Exposed read-only properties
    /// </summary>
    public bool IsPressed => isPressed;
    public float Horizontal => Mathf.Clamp((StickRect.anchoredPosition.x - defaultAnchoredPos.x) / Radius, -1f, 1f);
    public float Vertical => Mathf.Clamp((StickRect.anchoredPosition.y - defaultAnchoredPos.y) / Radius, -1f, 1f);
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    void Awake()
    {
        if (StickRect == null || BackgroundRect == null)
        {
            Debug.LogError("Assign StickRect and BackgroundRect on bl_Joystick.");
            enabled = false;
            return;
        }

        // find canvas
        m_Canvas = GetComponentInParent<Canvas>();
        if (m_Canvas == null)
        {
            Debug.LogError("Joystick must be child of a Canvas.");
            enabled = false;
            return;
        }

        m_Raycaster = m_Canvas.GetComponent<GraphicRaycaster>();
        if (m_Raycaster == null)
        {
            // not fatal — raycast still works via EventSystem
            // but better to warn:
            Debug.LogWarning("Canvas missing GraphicRaycaster (recommended).");
        }

        backImage = BackgroundRect.GetComponent<Image>();
        stickImage = StickRect.GetComponent<Image>();

        defaultAnchoredPos = BackgroundRect.anchoredPosition;
        StickRect.anchoredPosition = defaultAnchoredPos;
    }

    void Update()
    {
        if (!isPressed)
        {
            // Smoothly move stick back to center when released
            Vector2 cur = StickRect.anchoredPosition;
            StickRect.anchoredPosition = Vector2.SmoothDamp(cur, defaultAnchoredPos, ref currentVelocity, ReturnSmoothTime);
        }
    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        // record pointer id so other touches don't hijack this joystick
        if (pointerId == -1)
            pointerId = eventData.pointerId;
        else
            return;

        isPressed = true;

        // convert screen point to local point relative to BackgroundRect
        RectTransformUtility.ScreenPointToLocalPointInRectangle(BackgroundRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        pointerOffset = localPoint;

        UpdateStickPosition(localPoint);

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleStick(true));

        if (backImage != null) backImage.CrossFadeColor(PressColor, 0.08f, true, true);
        if (stickImage != null) stickImage.CrossFadeColor(PressColor, 0.08f, true, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(BackgroundRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        UpdateStickPosition(localPoint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        isPressed = false;
        pointerId = -1;

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleStick(false));

        if (backImage != null) backImage.CrossFadeColor(NormalColor, ScaleDuration, true, true);
        if (stickImage != null) stickImage.CrossFadeColor(NormalColor, ScaleDuration, true, true);
    }

    void UpdateStickPosition(Vector2 localPoint)
    {
        // localPoint is in local space of backgroundRect (pixels)
        Vector2 anchored = defaultAnchoredPos + localPoint;

        // clamp to radius
        Vector2 delta = anchored - defaultAnchoredPos;
        if (delta.magnitude > Radius)
            delta = delta.normalized * Radius;

        StickRect.anchoredPosition = defaultAnchoredPos + delta;
    }

    IEnumerator ScaleStick(bool enlarge)
    {
        float t = 0f;
        float dur = ScaleDuration;
        Vector3 start = StickRect.localScale;
        Vector3 end = enlarge ? Vector3.one * PressScale : Vector3.one;

        while (t < dur)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / dur);
            StickRect.localScale = Vector3.Lerp(start, end, alpha);
            yield return null;
        }
        StickRect.localScale = end;
    }
}
