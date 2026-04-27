using UnityEngine;

public class UIArrowBounce : MonoBehaviour
{
    public enum MoveType
    {
        UpDown,
        LeftRight,
        DiagonalUpRight,
        DiagonalUpLeft,
        Circular
    }

    [Header("Animation Type")]
    public MoveType moveType = MoveType.UpDown;

    [Header("Movement Settings")]
    public float distance = 10f;
    public float speed = 5f;

    [Header("Time Mode")]
    public bool useUnscaledTime = true;

    private RectTransform rt;
    private Vector2 startPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    void OnEnable()
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();

        startPos = rt.anchoredPosition;
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float wave = Mathf.Sin(t * speed) * distance;

        Vector2 offset = Vector2.zero;

        switch (moveType)
        {
            case MoveType.UpDown:
                offset = new Vector2(0f, wave);
                break;

            case MoveType.LeftRight:
                offset = new Vector2(wave, 0f);
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

        rt.anchoredPosition = startPos + offset;
    }
}