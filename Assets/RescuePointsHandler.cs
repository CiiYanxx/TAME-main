using UnityEngine;
using TMPro;

public class RescuePointsHandler : MonoBehaviour
{
    public static RescuePointsHandler Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI pointsText; 

    [Header("Points Settings")]
    public int currentPoints = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadPoints();
        UpdatePointsUI();
    }

    public void AddPoints(int amount)
    {
        // Eto ang tamang paraan para makita kung anong script ang tumatawag nito
        Debug.Log("<color=red>[POINTS DEBUG]</color> AddPoints called! Amount: " + amount, this.gameObject);
        
        currentPoints += amount;
        UpdatePointsUI();
        
        // Pwersahin ang pag-save agad sa JSON
        SavePointsToJSON();
    }

    public void DeductPoints(int amount)
    {
        currentPoints -= amount;
        if (currentPoints < 0) currentPoints = 0; 
        UpdatePointsUI();
        
        // PWERSAHIN ANG PAG-SAVE AGAD sa JSON
        SavePointsToJSON();
    }

    private void UpdatePointsUI()
    {
        if (pointsText != null)
        {
            // FIX: Number na lang ang lilitaw sa HUD
            pointsText.text = currentPoints.ToString();
        }
    }

    private void LoadPoints()
    {
        GameData data = SaveSystem.Load();
        if (data != null)
        {
            currentPoints = data.playerPoints; // Kunin ang points mula sa JSON
        }
    }

    // BAGONG FUNCTION PARA SIGURADONG HINDI MAG-ZERO ANG POINTS
    private void SavePointsToJSON()
    {
        GameData currentData = SaveSystem.Load();
        
        // Kunin ang mga existing data para hindi ma-overwrite ng maling values
        int missions = (currentData != null) ? currentData.completedMissions : 0;
        string name = (currentData != null) ? currentData.charName : "Rescue Hero";
        string appearance = (currentData != null) ? currentData.customizationData : "";
        
        // Default position mo kung sakaling wala pang save
        Vector3 pos = new Vector3(165.94f, 0.0f, 142.877f);
        if (currentData != null && currentData.playerPos[0] != 0)
        {
            pos = new Vector3(currentData.playerPos[0], currentData.playerPos[1], currentData.playerPos[2]);
        }

        // Tawagin ang 5 arguments ng Save function mo
        SaveSystem.Save(missions, currentPoints, pos, name, appearance);
        Debug.Log("Points Saved Permanently: " + currentPoints);
    }
}