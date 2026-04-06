[System.Serializable]
public class GameData {
    public int completedMissions;
    public int playerPoints;
    public float[] playerPos = new float[3];
    public string charName;
    public string customizationData;

    // --- MGA BAGONG DAGDAG ---
    public bool isMissionActive;      
    public int activeMissionIndex;    
}