using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static string SavePath => Application.persistentDataPath + "/savedata.json";

    public static void Save(int missions, int points, Vector3 position, string name, string appearance)
    {
        // 1. I-load muna ang existing data para hindi mawala yung 'isMissionActive' at iba pa
        GameData data = Load();
        
        // 2. Kung walang mahanap na save, gumawa ng bago
        if (data == null) 
        {
            data = new GameData();
        }

        // 3. Update lang natin yung specific fields na pinasa sa function
        data.completedMissions = missions;
        data.playerPoints = points; 
        data.playerPos[0] = position.x;
        data.playerPos[1] = position.y;
        data.playerPos[2] = position.z;
        data.charName = name;
        data.customizationData = appearance;

        // 4. Isulat na sa JSON
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game Saved to: " + SavePath);
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