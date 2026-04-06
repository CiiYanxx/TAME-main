using UnityEngine;

public class FadeObject : MonoBehaviour
{
    private Renderer targetRenderer;
    private float targetAlpha = 1.0f;
    private float currentAlpha = 1.0f;
    
    // Settings sa Inspector
    public float fadeSpeed = 5f;
    public float transparentLevel = 0.3f; // 30% transparency

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    // Ito ang tatawagin ng PlayerMovement
    public void FadeOut()
    {
        targetAlpha = transparentLevel;
    }

    void Update()
    {
        // Smooth transition the alpha
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        
        // I-apply sa material ng object.
        // Dapat ang Shader mo ay "Transparent" o may "_Color" property.
        targetRenderer.material.color = new Color(1, 1, 1, currentAlpha);

        // I-reset sa solid pagkatapos ng frame (kung hindi na tinatamaan ng SphereCast)
        targetAlpha = 1.0f; 
    }
}