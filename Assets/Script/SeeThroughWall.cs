using UnityEngine;

public class SeeThroughWall : MonoBehaviour
{
    public Transform player; // I-assign ang Player dito
    public float wallTransparency = 0.3f; // Gaano ka-transparent ang pader (0.0 to 1.0)
    
    private Renderer wallRenderer;
    private Shader originalShader;
    private Color originalColor;
    private bool isTransparent = false;

    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            originalShader = wallRenderer.material.shader;
            originalColor = wallRenderer.material.color;
        }
    }

    void Update()
    {
        if (player == null || wallRenderer == null) return;

        // Gumawa ng raycast mula sa camera patungo sa player
        Vector3 direction = player.position - Camera.main.transform.position;
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, direction, out hit))
        {
            // Kung ang tinamaan ng raycast ay ang pader na ito
            if (hit.collider.gameObject == gameObject)
            {
                if (!isTransparent)
                {
                    MakeWallTransparent();
                }
            }
            else
            {
                if (isTransparent)
                {
                    ResetWallMaterial();
                }
            }
        }
    }

    void MakeWallTransparent()
    {
        // Palitan ang shader sa isa na sumusuporta sa transparency (hal. Standard Shader na may Fade rendering mode)
        wallRenderer.material.shader = Shader.Find("Standard"); 
        
        // Siguraduhing naka-set ang Rendering Mode sa Fade o Transparent sa Material inspector
        
        Color transparentColor = originalColor;
        transparentColor.a = wallTransparency;
        wallRenderer.material.color = transparentColor;
        isTransparent = true;
    }

    void ResetWallMaterial()
    {
        wallRenderer.material.shader = originalShader;
        wallRenderer.material.color = originalColor;
        isTransparent = false;
    }
}