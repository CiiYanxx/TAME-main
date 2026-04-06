using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RescueController : MonoBehaviour
{
    public static RescueController Instance { get; private set; }
    
    [Header("UI Buttons")]
    public Button cleanButton; 
    public Button feedButton;
    public Button interactButton; 

    [Header("Settings")]
    public List<GameObject> animalPrefabs = new List<GameObject>();

    // --- DITO MO NA IA-ADJUST LAHAT ---
    [Header("Mobile Spawn Adjustments")]
    public float foodForwardOffset = 1.5f;   // Layo ng bowl sa player
    public float foodVerticalOffset = 0.1f;  // Taas ng bowl sa lupa
    public float debrisVerticalOffset = 0.1f; // Taas ng dumi sa lupa
    
    private GameObject currentSpawnedAnimal = null;
    private NPC activeQuestGiver = null;
    private QuestInfo currentQuestInfo = null;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (cleanButton != null) cleanButton.gameObject.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
    }

    public void StartMission(NPC questGiver, QuestInfo info) {
        CleanupMission();
        activeQuestGiver = questGiver;
        currentQuestInfo = info; 

        string targetName = info.targetAnimalName.ToLower().Trim();
        GameObject prefab = animalPrefabs.Find(p => p.name.ToLower().Trim() == targetName);

        if (prefab != null) {
            currentSpawnedAnimal = Instantiate(prefab, info.spawnPosition, Quaternion.Euler(info.animalRotation));
            AnimalMissionLogic logic = currentSpawnedAnimal.GetComponent<AnimalMissionLogic>();
            
            if (logic != null) {
                logic.cleanButton = this.cleanButton;
                logic.feedButton = this.feedButton;
                logic.interactButton = this.interactButton;
                logic.SetupMission(info);
            }
        }
    }

    public void ReportMissionOutcome(bool success) {
        if (success && currentQuestInfo != null) {
            if (RescuePointsHandler.Instance != null) {
                RescuePointsHandler.Instance.AddPoints(currentQuestInfo.progressPointsReward);
            }
        }
        if (activeQuestGiver != null) activeQuestGiver.ReportQuestOutcome(success);
        if (cleanButton != null) cleanButton.gameObject.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
        if (success) {
            if (currentSpawnedAnimal != null) Destroy(currentSpawnedAnimal);
            currentSpawnedAnimal = null;
        }
    }

    public void AddGravity(GameObject obj) {
        Rigidbody rb = obj.GetComponent<Rigidbody>() ?? obj.AddComponent<Rigidbody>();
        if (rb != null) {
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Mobile stability
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        if (obj.GetComponent<Collider>() == null) obj.AddComponent<BoxCollider>();
    }

    public void CleanupMission() {
        if (cleanButton != null) cleanButton.gameObject.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
        if (currentSpawnedAnimal != null) Destroy(currentSpawnedAnimal);
        currentSpawnedAnimal = null;
        currentQuestInfo = null; 
    }
}