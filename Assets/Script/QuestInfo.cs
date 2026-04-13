using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestInfo")]
public class QuestInfo : ScriptableObject {
    [Header("Animal Settings")]
    public string targetAnimalName = "Stray Cat";
    
    [Header("Preview UI Content")]
    [TextArea(3, 6)] 
    public string animalDetails = "Breed: \nColor: \nStatus: "; 
    [TextArea(5, 12)] 
    public string missionDescription = "Enter mission story and hints here..."; 

    public Vector3 spawnPosition;
    public Vector3 animalRotation; 

    [Header("Trust & Radius Settings (BAGO)")]
    public float trustDifficulty = 0.2f;
    [Tooltip("Laki ng yellow circle radius sa game.")]
    public float detectionRadius = 8.0f;

    [Header("Food Settings")]
    public GameObject foodBowlPrefab;

    [Header("UI & Logic")]
    public string questTitle = "Rescue Mission";
    public Sprite animalIcon;        
    public Sprite animalFullPreview; 
    public int locationIndex;
    public int missionIndex;
    public string acceptAnswer = "Good luck!";
    public int progressPointsReward = 10;

    [Header("Minigame Settings")]
    public int requiredSuccesses = 5;
    public int maxFailures = 3;
    public float pointerSpeed = 300f;
}