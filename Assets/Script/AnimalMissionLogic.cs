using UnityEngine;
using System.Collections;
using ithappy.Animals_FREE;

public class AnimalMissionLogic : MonoBehaviour
{
    public enum MissionStep { Waiting, BuildTrust, Feeding, Eating, FinishedEating }
    [Header("Mission Status")]
    public MissionStep currentStep = MissionStep.Waiting;

    [Header("Natural Roaming Settings")]
    public float wanderRadius = 6f;
    [Range(0.1f, 10f)]
    public float rotationSpeed = 3.5f; 
    [Range(0.1f, 2f)]
    public float walkSpeed = 0.35f;    
    public float stopDistance = 1.2f; 

    private Vector3 currentTarget;
    private Vector3 spawnPoint;

    private GameObject spawnedFoodBowl;
    private QuestInfo missionData;
    private AnimalInteractable animalInteract;
    private CreatureMover animalMover; 
    private float meterValue = 0.5f; // Nagsisimula sa gitna (0.5)
    private Transform playerTransform;
    private bool missionStarted = false;

    public void SetupMission(QuestInfo info)
    {
        missionData = info;
        animalInteract = GetComponent<AnimalInteractable>();
        animalMover = GetComponent<CreatureMover>();
        
        // Hanapin ang player
        if (PlayerMovement.Instance != null) playerTransform = PlayerMovement.Instance.transform;
        else playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        spawnPoint = transform.position; 

        if (animalInteract != null) animalInteract.SetupQuest(info); 

        // Setup Feed Button
        if (RescueController.Instance.feedButton != null) {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            RescueController.Instance.feedButton.onClick.RemoveAllListeners();
            RescueController.Instance.feedButton.onClick.AddListener(OnFeedButtonPressed);
        }

        PickNewTarget();
        meterValue = 0.5f; 
        currentStep = MissionStep.Waiting;
        missionStarted = true;
    }

    void Update() 
    { 
        if (!missionStarted || missionData == null || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (currentStep == MissionStep.Waiting)
        {
            HandleNaturalRoam();

            // LOGIC: Pag nasa loob ng Safe Zone (detectionRadius)
            if (distance <= missionData.detectionRadius) 
            {
                // Lumalapit = Taas (papuntang 1.0), Lumalayo = Baba (papuntang 0.1)
                float proximity = 1f - (distance / missionData.detectionRadius);
                meterValue = Mathf.Lerp(meterValue, Mathf.Clamp(proximity, 0.1f, 1f), Time.deltaTime * 5f);
            }
            else
            {
                // LOGIC: Pag lumabas sa Safe Zone (Penalty Drain mula sa current value)
                meterValue -= Time.deltaTime * missionData.drainSpeed;

                if (meterValue <= 0)
                {
                    meterValue = 0;
                    missionStarted = false;
                    RescueController.Instance.ReportMissionOutcome(false);
                    return;
                }
            }

            // Update UI sa RescueController
            Color stateColor = (meterValue < 0.3f) ? Color.red : (meterValue > 0.7f ? Color.green : Color.yellow);
            RescueController.Instance.UpdateNoiseMeter(true, stateColor, meterValue);
            
            // Transition kapag puno na ang meter at malapit na ang player
            if (meterValue >= 0.95f && distance <= 3f)
            {
                currentStep = MissionStep.BuildTrust;
                TransitionToFeeding();
            }
        }
    }

    private void HandleNaturalRoam()
    {
        if (animalMover == null) return;
        if (Vector3.Distance(transform.position, currentTarget) < 1.2f) PickNewTarget();

        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            direction.y = 0; 
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        animalMover.SetInput(new Vector2(0, walkSpeed), currentTarget, false, false);
    }

    private void PickNewTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        currentTarget = spawnPoint + new Vector3(randomPoint.x, 0, randomPoint.y);
    }

    private void TransitionToFeeding()
    {
        currentStep = MissionStep.Feeding;
        if (RescueController.Instance.sneakButton != null) RescueController.Instance.sneakButton.SetActive(false);
        if (RescueController.Instance.feedButton != null) RescueController.Instance.feedButton.gameObject.SetActive(true);
    }

    public void OnFeedButtonPressed() {
        if (currentStep == MissionStep.Feeding) {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            StartCoroutine(PerformFeedingSequence());
        }
    }

    IEnumerator PerformFeedingSequence()
    {
        Animator playerAnim = playerTransform.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.SetTrigger("Interact");
        yield return new WaitForSeconds(0.8f); 

        Vector3 spawnPos = playerTransform.position + (playerTransform.forward * RescueController.Instance.foodForwardOffset);
        spawnPos.y += RescueController.Instance.foodVerticalOffset;

        spawnedFoodBowl = Instantiate(missionData.foodBowlPrefab, spawnPos, playerTransform.rotation);
        RescueController.Instance.AddGravity(spawnedFoodBowl);
        if (animalInteract != null) animalInteract.SetFoodBowlReference(spawnedFoodBowl);

        yield return new WaitForSeconds(1.5f); 
        currentStep = MissionStep.Eating; 
        yield return StartCoroutine(AnimalWalkToFood(spawnPos));

        currentStep = MissionStep.FinishedEating;
        if (PointerController.Instance != null) PointerController.Instance.ShowTamePrompt(animalInteract);
    }

    IEnumerator AnimalWalkToFood(Vector3 targetPos)
    {
        if (animalMover == null) yield break;
        Vector3 stopPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        while (Vector3.Distance(transform.position, stopPos) > stopDistance) {
            float speedScale = Mathf.Clamp01(Vector3.Distance(transform.position, stopPos) / 2f);
            animalMover.SetInput(new Vector2(0, 1 * speedScale), stopPos, false, false);
            yield return null;
        }
        animalMover.SetInput(Vector2.zero, transform.position, false, false);
    }

    // ITO YUNG NAWALA: Function para sa PlayerInteraction.cs
    public void OnPlayerInteract() 
    { 
        Debug.Log("Player interacted with " + gameObject.name); 
    }

    public void UpdateDebrisDetection(DebrisItem d, bool b) { }

    public float GetTrustPercentage() { return meterValue; }
}