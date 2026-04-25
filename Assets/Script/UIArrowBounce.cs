using UnityEngine;

public class UIArrowBounce : MonoBehaviour
{
    public float bounce = 10f;
    public float speed = 5f;

    RectTransform rt;
    Vector2 startPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    void Update()
    {
        rt.anchoredPosition = startPos +
            new Vector2(0, Mathf.Sin(Time.unscaledTime * speed) * bounce);
    }
}