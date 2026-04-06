using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static string SavePath => Application.persistentDataPath + "/savedata.json";

    public static void Save(int missions, int points, Vector3 position, string name, string appearance)
    {
        GameData data = new GameData();
        data.completedMissions = missions;
        data.playerPoints = points; 
        data.playerPos[0] = position.x;
        data.playerPos[1] = position.y;
        data.playerPos[2] = position.z;
        data.charName = name;
        data.customizationData = appearance;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
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

    public static bool HasSave() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
// DAPAT WALA NANG NAKALAGAY NA 'public class GameData' DITO SA BABA