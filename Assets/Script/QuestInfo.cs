using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct DebrisSpawnData {
    public GameObject debrisPrefab;
    public Vector3 offset;    
    public Vector3 rotation;  
}

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestInfo")]
public class QuestInfo : ScriptableObject {
    [Header("Animal Settings")]
    [Tooltip("This name MUST match the name of the prefab in the RescueController list.")]
    public string targetAnimalName = "Stray Cat";
    
    [Header("Preview UI Content")]
    [TextArea(3, 6)] 
    public string animalDetails = "Breed: \nColor: \nStatus: "; 

    [TextArea(5, 12)] 
    public string missionDescription = "Enter mission story and hints here..."; 

    public Vector3 spawnPosition;
    public Vector3 animalRotation; 

    [Header("Debris List")]
    public List<DebrisSpawnData> debrisLocations;

    [Header("Food Settings")]
    public GameObject foodBowlPrefab;

    [Header("UI & Logic")]
    public string questTitle = "Rescue Mission";
    public Sprite animalIcon;        // Yung naka-zoom sa mukha
    public Sprite animalFullPreview; // BAGO: Yung whole body na may BG
    public int locationIndex;
    public int missionIndex;
    public string acceptAnswer = "Good luck!";
    public int progressPointsReward = 10;

    [Header("Minigame Settings")]
    public int requiredSuccesses = 5;
    public int maxFailures = 3;
    public float pointerSpeed = 300f;
}