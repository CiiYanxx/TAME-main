using UnityEngine;
using UnityEngine.UI;
using TMPro; // Still included, but no longer used

public class RescuePointsBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Image component that will be the circular bar.")]
    public Image fillImage;
    // Removed: public TextMeshProUGUI valueText;

    [Header("Progress Settings")]
    [Tooltip("The player's current Rescue Points.")]
    public float currentPoints = 0f;
    [Tooltip("The total Rescue Points required for completion (e.g., leveling up).")]
    public float maxPoints = 1000f;

    void Start()
    {
        // Ensure the Image component is set to Filled type in the Inspector
        if (fillImage != null && fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("Fill Image Type is not set to 'Filled'! Please change it in the Inspector for circular functionality.");
        }

        // Initialize the display
        UpdateBarDisplay();
    }

    /// <summary>
    /// Updates the bar's visual fill only.
    /// </summary>
    private void UpdateBarDisplay()
    {
        // Calculate the fill amount (a value between 0.0 and 1.0)
        float fillAmount = Mathf.Clamp01(currentPoints / maxPoints);
        
        // Apply the fill amount to the circular Image
        if (fillImage != null)
        {
            fillImage.fillAmount = fillAmount;
        }
        else
        {
            Debug.LogError("Fill Image reference is missing on RescuePointsBar!");
        }

        // Removed: Logic to update the text display
    }

    /// <summary>
    /// Public method to add Rescue Points and refresh the bar.
    /// </summary>
    /// <param name="pointsToAdd">The amount of points earned.</param>
    public void AddRescuePoints(int pointsToAdd)
    {
        currentPoints += pointsToAdd;
        
        // Handle overflow/level-up logic if needed
        if (currentPoints >= maxPoints)
        {
            // Example: If progress exceeds max, handle level-up here.
            Debug.Log("Rescue Points Max Reached! Implement Level Up/Reset logic here.");
            currentPoints = maxPoints; // Cap at max for display purposes
        }

        UpdateBarDisplay();
    }

    /// <summary>
    /// Public method to deduct Rescue Points and refresh the bar.
    /// </summary>
    /// <param name="pointsToDeduct">The amount of points lost.</param>
    public void DeductRescuePoints(int pointsToDeduct)
    {
        currentPoints -= pointsToDeduct;
        currentPoints = Mathf.Max(0, currentPoints); // Prevent going below zero

        UpdateBarDisplay();
    }
}