using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class RescueController : MonoBehaviour
{
    public static RescueController Instance { get; private set; }

    [Header("Noise Meter UI")]
    public GameObject noiseMeterGroup; // Background/Parent ng meter
    public Image noiseMeterFill;       // Meter Line (Image Component, Filled, Vertical)
    public GameObject sneakButton;
    public Button feedButton;

    [Header("Settings & Prefabs")]
    public List<GameObject> animalPrefabs = new List<GameObject>();
    
    // Eto yung nawalang variables kaya ka nag-eerror:
    public float foodForwardOffset = 1.5f;   
    public float foodVerticalOffset = 0.1f;  
    
    private GameObject currentAnimal = null;
    private NPC activeNPC = null;
    private QuestInfo currentInfo = null;
    private AnimalMissionLogic activeMissionLogic;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);

        CleanupMission(); 
    }

    public void StartMission(NPC npc, QuestInfo info) 
    {
        CleanupMission();
        activeNPC = npc;
        currentInfo = info;

        if (sneakButton != null) sneakButton.SetActive(true);

        // Hanapin ang prefab base sa pangalan
        GameObject prefab = animalPrefabs.Find(p => p.name.ToLower().Trim() == info.targetAnimalName.ToLower().Trim());
        
        if (prefab != null) {
            currentAnimal = Instantiate(prefab, info.spawnPosition, Quaternion.Euler(info.animalRotation));
            
            // Setup ang mga logic components sa na-spawn na hayop
            activeMissionLogic = currentAnimal.GetComponent<AnimalMissionLogic>();
            if (activeMissionLogic == null) activeMissionLogic = currentAnimal.AddComponent<AnimalMissionLogic>();
            
            activeMissionLogic.SetupMission(info);
            
            // Setup ang interactable visual circle
            AnimalInteractable interactable = currentAnimal.GetComponent<AnimalInteractable>();
            if (interactable != null) interactable.SetupQuest(info);
        }
    }

    // UpdateNoiseMeter na tumatanggap ng fillValue (0 to 1)
    public void UpdateNoiseMeter(bool isVisible, Color stateColor, float fillValue) 
    {
        if (noiseMeterGroup == null || noiseMeterFill == null) return;
        
        if (noiseMeterGroup.activeSelf != isVisible) {
            noiseMeterGroup.SetActive(isVisible);
        }

        if (!isVisible) return;

        // Image fill control para sa Vertical movement
        noiseMeterFill.fillAmount = fillValue;
        noiseMeterFill.color = stateColor;
    }

    public void ReportMissionOutcome(bool success) 
    {
        if (success && currentInfo != null) {
            RescuePointsHandler.Instance?.AddPoints(currentInfo.progressPointsReward);
        }

        if (activeNPC != null) activeNPC.ReportQuestOutcome(success);

        if (noiseMeterGroup != null) noiseMeterGroup.SetActive(false);
        if (sneakButton != null) sneakButton.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);

        StartCoroutine(CleanupAfterDelay());
    }

    IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        CleanupMission();
    }

    public void CleanupMission() 
    {
        if (noiseMeterGroup != null) noiseMeterGroup.SetActive(false);
        if (sneakButton != null) sneakButton.SetActive(false);
        if (feedButton != null) feedButton.gameObject.SetActive(false);
        
        if (noiseMeterFill != null) noiseMeterFill.fillAmount = 0;

        if (currentAnimal != null) Destroy(currentAnimal);

        currentAnimal = null;
        activeMissionLogic = null;
    }

    public void AddGravity(GameObject obj) 
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>() ?? obj.AddComponent<Rigidbody>();
        rb.useGravity = true;
        if (obj.GetComponent<Collider>() == null) obj.AddComponent<BoxCollider>();
    }
}