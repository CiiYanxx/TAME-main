using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestData", menuName = "ScriptableObjects/QuestInfo")]
public class QuestInfo : ScriptableObject {
    [Header("Basic Animal Info")]
    public string targetAnimalName = "Stray Cat";
    public Sprite animalIcon;        
    public Sprite animalFullPreview; 

    [Header("Preview UI Content")]
    [TextArea(3, 6)] 
    public string animalDetails = "Breed: \nColor: \nStatus: "; 
    [TextArea(5, 12)] 
    public string missionDescription = "Enter mission story and hints here..."; 

    [Header("Spawn Settings")]
    public GameObject animalPrefab; 
    public Vector3 spawnPosition;
    public Vector3 animalRotation; 

    [Header("Distance & Detection")]
    [Tooltip("Safe Zone Radius. Pag lumabas ka rito, babagsak ang meter.")]
    public float detectionRadius = 10.0f;
    [Tooltip("Distance where animal flees if NOT sneaking.")]
    public float fleeRadius = 12.0f; 
    [Tooltip("Gaano kabilis bumagsak ang meter pag lumabas sa radius.")]
    public float drainSpeed = 0.2f;

    [Header("Food & Feeding")]
    public GameObject foodBowlPrefab;

    [Header("Progression Logic")]
    public string questTitle = "Rescue Mission";
    public int locationIndex; 
    public int missionIndex;
    public int progressPointsReward = 10;
    public string acceptAnswer = "Good luck!";

    [Header("Minigame (Taming) Settings")]
    public int requiredSuccesses = 5;
    public int maxFailures = 3;
    public float pointerSpeed = 300f;
}