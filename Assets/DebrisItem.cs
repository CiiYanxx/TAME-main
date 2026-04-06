using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisItem : MonoBehaviour
{
    private List<Material> materials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    public Color highlightColor = Color.yellow;
    public float dissolveSpeed = 2f;
    private bool isDissolving = false;

    void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            materials.Add(r.material);
            originalColors.Add(r.material.color);
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (isDissolving) return;
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].color = isHighlighted ? highlightColor : originalColors[i];
        }
    }

    public void StartDissolve()
    {
        if (isDissolving) return;
        isDissolving = true;
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        float progress = 1f;
        while (progress > 0)
        {
            progress -= Time.deltaTime * dissolveSpeed;
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    Color c = mat.color;
                    c.a = progress;
                    mat.color = c;
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}