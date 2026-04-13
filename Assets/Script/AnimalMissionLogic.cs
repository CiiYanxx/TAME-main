using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ithappy.Animals_FREE;

public class AnimalMissionLogic : MonoBehaviour
{
    public enum MissionStep { BuildTrust, Feeding, Eating, FinishedEating, Minigame }
    [Header("Mission Status")]
    public MissionStep currentStep = MissionStep.BuildTrust;

    [Header("Distance Settings")]
    public float stopDistance = 1.2f; 

    [HideInInspector] public Button cleanButton; // Placeholder to avoid controller errors
    [HideInInspector] public Button feedButton;
    [HideInInspector] public Button interactButton; 
    
    private GameObject spawnedFoodBowl;
    private QuestInfo missionData;
    private AnimalInteractable animalInteract;
    private CreatureMover animalMover; 
    private float trustLevel = 0f;
    private Transform playerTransform;
    private LineRenderer circleRenderer;

    public void SetupMission(QuestInfo info)
    {
        missionData = info;
        animalInteract = GetComponent<AnimalInteractable>();
        animalMover = GetComponent<CreatureMover>();
        playerTransform = PlayerMovement.Instance.transform;
        
        if (animalInteract != null) {
            animalInteract.SetupQuest(info); 
            animalInteract.enabled = false;
        }

        // Setup the Visual Radius Line
        SetupCircleRenderer();

        if (RescueController.Instance.feedButton != null) {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            RescueController.Instance.feedButton.onClick.RemoveAllListeners();
            RescueController.Instance.feedButton.onClick.AddListener(OnFeedButtonPressed);
        }

        currentStep = MissionStep.BuildTrust;
    }

    private void SetupCircleRenderer()
    {
        circleRenderer = gameObject.AddComponent<LineRenderer>();
        circleRenderer.useWorldSpace = false;
        circleRenderer.loop = true;
        circleRenderer.positionCount = 51;
        circleRenderer.startWidth = 0.04f; // Manipis na linya
        circleRenderer.endWidth = 0.04f;
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.startColor = new Color(1f, 0.92f, 0.016f, 0.4f); // Yellow half-transparent
        circleRenderer.endColor = new Color(1f, 0.92f, 0.016f, 0.4f);

        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * missionData.detectionRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * missionData.detectionRadius;
            circleRenderer.SetPosition(i, new Vector3(x, 0.05f, z)); // Konting taas para di lubog
            angle += 360f / 50;
        }
    }

    void Update() 
    { 
        if (currentStep == MissionStep.BuildTrust) HandleTrustLogic(); 
    }

    private void HandleTrustLogic()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Ang Noise Meter ay lilitaw lang pag nasa loob ng radius line
        if (distance <= missionData.detectionRadius)
        {
            if (PlayerMovement.Instance.isRunning) trustLevel -= Time.deltaTime * 0.4f;
            else if (PlayerMovement.Instance.isSneaking) trustLevel += Time.deltaTime * missionData.trustDifficulty;

            trustLevel = Mathf.Clamp01(trustLevel);
            RescueController.Instance.UpdateTrustUI(trustLevel, true);
        }
        else 
        {
            RescueController.Instance.UpdateTrustUI(trustLevel, false);
        }

        if (trustLevel >= 0.95f && distance <= 3.5f) {
            if (RescueController.Instance.feedButton != null && !RescueController.Instance.feedButton.gameObject.activeSelf) {
                RescueController.Instance.feedButton.gameObject.SetActive(true);
                RescueController.Instance.UpdateTrustUI(trustLevel, false);
                if (circleRenderer != null) circleRenderer.enabled = false; // Hide pag panalo na
            }
        }
    }

    public void OnFeedButtonPressed() {
        if (currentStep == MissionStep.BuildTrust && trustLevel >= 0.9f) {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            currentStep = MissionStep.Feeding;
            StartCoroutine(PerformFeedingSequence());
        }
    }

    // Original functions kept to avoid errors
    public void OnPlayerInteract() { }
    public void UpdateDebrisDetection(DebrisItem d, bool b) { }
    public void OnCleanButtonPressed() { }

    IEnumerator PerformFeedingSequence()
    {
        Animator playerAnim = PlayerMovement.Instance.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.SetTrigger("Interact");
        yield return new WaitForSeconds(0.8f); 

        Transform playerT = PlayerMovement.Instance.transform;
        Vector3 spawnPos = playerT.position + (playerT.forward * RescueController.Instance.foodForwardOffset);
        spawnPos.y += RescueController.Instance.foodVerticalOffset;

        spawnedFoodBowl = Instantiate(missionData.foodBowlPrefab, spawnPos, playerT.rotation);
        RescueController.Instance.AddGravity(spawnedFoodBowl);
        if (animalInteract != null) animalInteract.SetFoodBowlReference(spawnedFoodBowl);

        yield return new WaitForSeconds(2.0f); 
        currentStep = MissionStep.Eating; 
        yield return StartCoroutine(AnimalWalkToFood(spawnPos));

        currentStep = MissionStep.FinishedEating;
        if (animalInteract != null) animalInteract.enabled = true;
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