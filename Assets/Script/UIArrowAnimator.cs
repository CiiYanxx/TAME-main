using UnityEngine;

public class UIArrowAnimator : MonoBehaviour
{
    public enum MoveType
    {
        None,
        UpDown,
        LeftRight,
        DiagonalUpRight,
        DiagonalUpLeft,
        Circular
    }

    [Header("Animation")]
    public MoveType moveType = MoveType.UpDown;

    public float speed = 5f;
    public float distance = 10f;
    public bool useUnscaledTime = true;

    private RectTransform rect;
    private Vector2 startPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    void OnEnable()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        startPos = rect.anchoredPosition;
    }

    void Update()
    {
        if (rect == null) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float wave = Mathf.Sin(t * speed) * distance;

        Vector2 offset = Vector2.zero;

        switch (moveType)
        {
            case MoveType.UpDown:
                offset = new Vector2(0, wave);
                break;

            case MoveType.LeftRight:
                offset = new Vector2(wave, 0);
                break;

            case MoveType.DiagonalUpRight:
                offset = new Vector2(wave, wave);
                break;

            case MoveType.DiagonalUpLeft:
                offset = new Vector2(-wave, wave);
                break;

            case MoveType.Circular:
                offset = new Vector2(
                    Mathf.Cos(t * speed) * distance,
                    Mathf.Sin(t * speed) * distance
                );
                break;
        }

        rect.anchoredPosition = startPos + offset;
    }
}