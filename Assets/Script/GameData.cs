[System.Serializable]
public class GameData {
    public int completedMissions;
    public int playerPoints;
    public float[] playerPos = new float[3];
    public string charName;
    public string customizationData;

    // --- MGA BAGONG DAGDAG ---
    public bool isMissionActive;      // Para malaman kung may tinanggap na mission
    public int activeMissionIndex;    // Para malaman kung anong mission ang active
}