using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static string SavePath => Application.persistentDataPath + "/savedata.json";

    public static void Save(int missions, int points, Vector3 position, string name, string appearance)
    {
        GameData data = Load();
        
        if (data == null) 
        {
            data = new GameData();
        }

        // SIGURADUHIN: Na ang array ay may laman bago sulatan
        if (data.playerPos == null || data.playerPos.Length != 3)
        {
            data.playerPos = new float[3];
        }

        data.completedMissions = missions;
        data.playerPoints = points; 
        data.playerPos[0] = position.x;
        data.playerPos[1] = position.y;
        data.playerPos[2] = position.z;
        data.charName = name;
        data.customizationData = appearance;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("<color=yellow>[SAVESYSTEM]</color> Saved successfully to JSON.");
    }

    public static GameData Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    public static bool HasSave() 
    {
        return File.Exists(Application.persistentDataPath + "/savedata.json");
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}