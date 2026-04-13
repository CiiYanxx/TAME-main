using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RescueController : MonoBehaviour
{
    public static RescueController Instance { get; private set; }
    
    [Header("UI References")]
    public Slider trustSlider; 
    public Button feedButton;
    public Button interactButton; 
    public GameObject sneakButton; // I-drag dito ang Sneak Button UI object

    [Header("Settings")]
    public List<GameObject> animalPrefabs = new List<GameObject>();

    [Header("Mobile Spawn Adjustments")]
    public float foodForwardOffset = 1.5f;   
    public float foodVerticalOffset = 0.1f;  
    
    private GameObject currentSpawnedAnimal = null;
    private NPC activeQuestGiver = null;
    private QuestInfo currentQuestInfo = null;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (trustSlider != null) trustSlider.gameObject.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
        if (sneakButton != null) sneakButton.SetActive(false); // Hide sa simula
    }

    public void StartMission(NPC questGiver, QuestInfo info) {
        CleanupMission();
        activeQuestGiver = questGiver;
        currentQuestInfo = info; 

        // Ipakita ang sneak button dahil nagsimula na ang mission
        if (sneakButton != null) sneakButton.SetActive(true);

        string targetName = info.targetAnimalName.ToLower().Trim();
        GameObject prefab = animalPrefabs.Find(p => p.name.ToLower().Trim() == targetName);

        if (prefab != null) {
            currentSpawnedAnimal = Instantiate(prefab, info.spawnPosition, Quaternion.Euler(info.animalRotation));
            AnimalMissionLogic logic = currentSpawnedAnimal.GetComponent<AnimalMissionLogic>();
            if (logic != null) logic.SetupMission(info);
        }
    }

    public void UpdateTrustUI(float value, bool isVisible) {
        if (trustSlider != null) {
            if(trustSlider.gameObject.activeSelf != isVisible) trustSlider.gameObject.SetActive(isVisible);
            trustSlider.value = value;
        }
    }

    public void ReportMissionOutcome(bool success) {
        if (success && currentQuestInfo != null) {
            if (RescuePointsHandler.Instance != null) 
                RescuePointsHandler.Instance.AddPoints(currentQuestInfo.progressPointsReward);
        }
        
        if (activeQuestGiver != null) activeQuestGiver.ReportQuestOutcome(success);
        
        CleanupMission(); // Dito rin itatago ang sneak button
    }

    public void AddGravity(GameObject obj) {
        Rigidbody rb = obj.GetComponent<Rigidbody>() ?? obj.AddComponent<Rigidbody>();
        if (rb != null) {
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        if (obj.GetComponent<Collider>() == null) obj.AddComponent<BoxCollider>();
    }

    public void CleanupMission() {
        if (trustSlider != null) trustSlider.gameObject.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
        if (sneakButton != null) sneakButton.SetActive(false); // Itago ang sneak button

        if (currentSpawnedAnimal != null) Destroy(currentSpawnedAnimal);
        currentSpawnedAnimal = null;
        currentQuestInfo = null; 
    }
}