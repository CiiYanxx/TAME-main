using UnityEngine;

public class ProgressSystem : MonoBehaviour
{
    public static ProgressSystem Instance { get; private set; }

    [Header("UI Reference")]
    // Reference to the script that manages the visual bar
    public RescuePointsBar rescuePointsBar; 
    // Add reference for a coin display if one exists (for completeness)
    // public TextMeshProUGUI coinDisplay;

    [Header("Progress & Currency")]
    public int rescueProgressPoints = 0;
    public int goldCoins = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 🚨 REVISION: Removed Start() method. The RescuePointsBar handles its own initialization in its Start().

    public void AddProgress(int points)
    {
        if (points > 0)
        {
            // 1. Update the internal state
            rescueProgressPoints += points;
            Debug.Log($"Gained {points} Progress Points. Total: {rescueProgressPoints}");

            // 2. Update the UI bar
            if (rescuePointsBar != null)
            {
                // The bar handles clamping/level-up visuals internally
                rescuePointsBar.AddRescuePoints(points); 
            }
        }
    }
    
    public void DeductProgress(int points)
    {
        if (points > 0)
        {
            // 1. Update the internal state
            rescueProgressPoints -= points;
            rescueProgressPoints = Mathf.Max(0, rescueProgressPoints); // Prevent going below zero
            Debug.LogWarning($"Deducted {points} Progress Points. Total: {rescueProgressPoints}");

            // 2. Update the UI bar
            if (rescuePointsBar != null)
            {
                rescuePointsBar.DeductRescuePoints(points);
            }
        }
    }

    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            goldCoins += amount;
            Debug.Log($"Gained {amount} Gold Coins. Total: {goldCoins}");
            // Update Coin UI here if you had a display reference
            // if (coinDisplay != null) coinDisplay.text = goldCoins.ToString();
        }
    }
}