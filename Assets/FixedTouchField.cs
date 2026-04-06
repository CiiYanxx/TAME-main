using UnityEngine;
using UnityEngine.EventSystems;

public class FixedTouchField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [HideInInspector] public Vector2 TouchDist;
    [HideInInspector] public bool Pressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Pressed)
        {
            // Ito ang magbabago ng X at Y values mo sa Editor at Mobile
            TouchDist = eventData.delta;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
        TouchDist = Vector2.zero;
    }

    private void LateUpdate()
    {
        // I-reset para hindi mag-stuck ang camera rotation
        TouchDist = Vector2.zero;
    }
}