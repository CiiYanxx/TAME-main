using UnityEngine;

public class MapController : MonoBehaviour
{
    [Tooltip("Drag the MapPanel GameObject here.")]
    public GameObject mapPanel;

    // Call this method when the MapButton is pressed
    public void ToggleMap()
    {
        if (mapPanel != null)
        {
            // Toggles the active state of the panel
            bool isActive = mapPanel.activeSelf;
            mapPanel.SetActive(!isActive);

            // Optional: Pause the game when the map is open
            Time.timeScale = isActive ? 1f : 0f;
        }
    }
}