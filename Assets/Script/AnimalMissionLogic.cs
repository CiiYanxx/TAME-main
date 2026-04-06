using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ithappy.Animals_FREE;

public class AnimalMissionLogic : MonoBehaviour
{
    public enum MissionStep { ClearDebris, Feeding, Eating, FinishedEating, Minigame }
    [Header("Mission Status")]
    public MissionStep currentStep = MissionStep.ClearDebris;

    [Header("Distance Settings")]
    [Tooltip("Gaano kalayo hihinto ang hayop sa pagkain.")]
    public float stopDistance = 1.2f; 

    [Header("UI Buttons (Assigned by RescueController)")]
    public Button cleanButton; 
    public Button feedButton;
    [HideInInspector] public Button interactButton; 
    
    [Header("Detection Settings")]
    public float detectionRange = 4.0f; 

    private List<DebrisItem> debrisInRoom = new List<DebrisItem>(); 
    private DebrisItem currentTargetDebris;
    private GameObject spawnedFoodBowl;
    private QuestInfo missionData;
    private AnimalInteractable animalInteract;
    private CreatureMover animalMover; 

    public void SetupMission(QuestInfo info)
    {
        missionData = info;
        animalInteract = GetComponent<AnimalInteractable>();
        animalMover = GetComponent<CreatureMover>();
        
        // Ipinapasa ang QuestInfo sa AnimalInteractable para makuha ng PointerController (Fixed 8/3)
        if (animalInteract != null) {
            animalInteract.SetupQuest(info); 
            animalInteract.enabled = false;
        }

        if (cleanButton != null) {
            cleanButton.gameObject.SetActive(true);
            cleanButton.onClick.RemoveAllListeners();
            cleanButton.onClick.AddListener(OnCleanButtonPressed);
        }

        if (feedButton != null) {
            feedButton.gameObject.SetActive(false);
            feedButton.onClick.RemoveAllListeners();
            feedButton.onClick.AddListener(OnFeedButtonPressed);
        }

        foreach (DebrisSpawnData data in info.debrisLocations)
        {
            if (data.debrisPrefab != null)
            {
                Vector3 spawnPos = transform.position + data.offset;
                if (RescueController.Instance != null) spawnPos.y += RescueController.Instance.debrisVerticalOffset;

                GameObject dObj = Instantiate(data.debrisPrefab, spawnPos, Quaternion.Euler(data.rotation));
                if (RescueController.Instance != null) RescueController.Instance.AddGravity(dObj);
                
                DebrisItem item = dObj.GetComponent<DebrisItem>() ?? dObj.AddComponent<DebrisItem>();
                debrisInRoom.Add(item);
            }
        }
    }

    void Update() { if (currentStep == MissionStep.ClearDebris) HighlightNearest(); }

    public void UpdateDebrisDetection(DebrisItem debris, bool isEntering) { HighlightNearest(); }

    public void OnCleanButtonPressed()
    {
        if (currentStep != MissionStep.ClearDebris || currentTargetDebris == null) return;
        DebrisItem toRemove = currentTargetDebris;
        debrisInRoom.Remove(toRemove);
        toRemove.StartDissolve(); 
        currentTargetDebris = null;
        if (debrisInRoom.Count == 0) 
        {
            currentStep = MissionStep.Feeding;
            if (cleanButton != null) cleanButton.gameObject.SetActive(false);
            if (feedButton != null) feedButton.gameObject.SetActive(true); 
        }
    }

    public void HighlightNearest()
    {
        debrisInRoom.RemoveAll(d => d == null);
        DebrisItem nearest = null;
        float minDistance = detectionRange;
        Vector3 playerPos = PlayerMovement.Instance.transform.position;
        foreach (DebrisItem d in debrisInRoom)
        {
            float dist = Vector3.Distance(playerPos, d.transform.position);
            if (dist < minDistance) { minDistance = dist; nearest = d; }
        }
        if (currentTargetDebris != null && currentTargetDebris != nearest) currentTargetDebris.SetHighlight(false);
        if (nearest != null) { currentTargetDebris = nearest; currentTargetDebris.SetHighlight(true); }
        else { currentTargetDebris = null; }
    }

    public void OnFeedButtonPressed()
    {
        if (currentStep == MissionStep.Feeding) 
        {
            if (feedButton != null) feedButton.gameObject.SetActive(false);
            StartCoroutine(PerformFeedingSequence());
        }
    }

    // --- FIX: DINAGDAG ITO PARA MAWALA ANG ERROR SA PLAYERINTERACTION ---
    public void OnPlayerInteract() 
    { 
        // Pwedeng walang laman ito kung Buttons naman ang gamit mo sa Clear Debris at Feed.
        // Pero kailangan ito ng PlayerInteraction script para mag-compile.
    }

    IEnumerator PerformFeedingSequence()
    {
        Animator playerAnim = PlayerMovement.Instance.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.SetTrigger("Interact");
        yield return new WaitForSeconds(0.8f); 

        Transform playerT = PlayerMovement.Instance.transform;
        float fwd = (RescueController.Instance != null) ? RescueController.Instance.foodForwardOffset : 1.5f;
        float vrt = (RescueController.Instance != null) ? RescueController.Instance.foodVerticalOffset : 0.1f;

        Vector3 spawnPos = playerT.position + (playerT.forward * fwd);

        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out hit, 5f)) {
            spawnPos.y = hit.point.y + vrt;
        } else {
            spawnPos.y = playerT.position.y + vrt; 
        }

        spawnedFoodBowl = Instantiate(missionData.foodBowlPrefab, spawnPos, playerT.rotation);
        if (RescueController.Instance != null) RescueController.Instance.AddGravity(spawnedFoodBowl);
        if (animalInteract != null) animalInteract.SetFoodBowlReference(spawnedFoodBowl);

        yield return new WaitForSeconds(2.0f); 
        currentStep = MissionStep.Eating; 
        yield return StartCoroutine(AnimalWalkToFood(spawnPos));

        currentStep = MissionStep.FinishedEating;
        if (animalInteract != null) animalInteract.enabled = true;
        
        // Tatawag sa PointerController gamit ang QuestInfo na na-setup na
        if (PointerController.Instance != null) PointerController.Instance.ShowTamePrompt(animalInteract);
    }

    IEnumerator AnimalWalkToFood(Vector3 targetPos)
    {
        if (animalMover == null) yield break;
        Vector3 stopPos = targetPos; 
        stopPos.y = transform.position.y;

        float currentDistance = Vector3.Distance(transform.position, stopPos);
        while (currentDistance > stopDistance)
        {
            currentDistance = Vector3.Distance(transform.position, stopPos);
            float speedScale = Mathf.Clamp01((currentDistance - (stopDistance * 0.5f)) / stopDistance);
            animalMover.SetInput(new Vector2(0, 1 * speedScale), stopPos, false, false);
            yield return null;
        }
        animalMover.SetInput(Vector2.zero, transform.position + transform.forward, false, false);
        yield return new WaitForSeconds(2.5f); 
    }
}