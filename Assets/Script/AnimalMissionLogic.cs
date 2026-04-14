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
    [Range(0.1f, 10f)] public float rotationSpeed = 5f; 
    [Range(0.1f, 2f)] public float walkSpeed = 0.35f;    
    public float stopDistance = 0.8f; 

    private Vector3 currentTarget;
    private Vector3 spawnPoint;
    private GameObject spawnedFoodBowl;
    private QuestInfo missionData;
    private AnimalInteractable animalInteract;
    private CreatureMover animalMover; 
    private float meterValue = 0.5f; 
    private Transform playerTransform;
    private bool missionStarted = false;
    private bool hasTouchedRadius = false;

    public void SetupMission(QuestInfo info)
    {
        missionData = info;
        animalInteract = GetComponent<AnimalInteractable>();
        animalMover = GetComponent<CreatureMover>();
        
        if (PlayerMovement.Instance != null) playerTransform = PlayerMovement.Instance.transform;
        else playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        spawnPoint = transform.position; 

        if (animalInteract != null) animalInteract.SetupQuest(info); 

        if (RescueController.Instance.feedButton != null) 
        {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            RescueController.Instance.feedButton.onClick.RemoveAllListeners();
            RescueController.Instance.feedButton.onClick.AddListener(OnFeedButtonPressed);
        }

        PickNewTarget();
        meterValue = 0.5f; 
        currentStep = MissionStep.Waiting;
        missionStarted = true;
        hasTouchedRadius = false;
    }

    public void OnPlayerInteract()
    {
        if (currentStep == MissionStep.Feeding) 
            OnFeedButtonPressed();
    }

    void Update() 
    { 
        if (!missionStarted || missionData == null || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (currentStep == MissionStep.Waiting || currentStep == MissionStep.BuildTrust)
        {
            if (distance <= 10f && PlayerMovement.Instance.isSneaking)
            {
                hasTouchedRadius = true;
                currentStep = MissionStep.BuildTrust;

                LookAtPlayer();

                if (animalMover != null) 
                    animalMover.SetInput(Vector2.zero, transform.position, false, false);

                float rawProximity = Mathf.InverseLerp(10f, 5f, distance);
                float targetValue = Mathf.Lerp(0.5f, 1.0f, rawProximity);

                meterValue = Mathf.MoveTowards(meterValue, targetValue, Time.deltaTime * 2f);
            }
            else
            {
                HandleNaturalRoam(); 

                if (hasTouchedRadius)
                {
                    meterValue -= Time.deltaTime * missionData.drainSpeed;

                    if (meterValue <= 0)
                    {
                        HandleFleeSequence();
                        return;
                    }
                }
            }

            if (distance <= 4f && !PlayerMovement.Instance.isSneaking)
            {
                HandleFleeSequence();
                return;
            }

            if (hasTouchedRadius)
            {
                Color stateColor = Color.yellow;
                if (meterValue < 0.35f) stateColor = Color.red;
                else if (meterValue >= 1.0f) stateColor = Color.green;

                RescueController.Instance.UpdateNoiseMeter(true, stateColor, meterValue);
            }

            if (meterValue >= 1.0f && distance <= 5.5f) 
            {
                currentStep = MissionStep.Feeding;
                StartCoroutine(TransitionToFeedingSequence());
            }
        }
    }

    private void HandleFleeSequence()
    {
        missionStarted = false;

        CleanupFoodBowl(); // 🔥 FAIL CLEANUP

        if (PlayerMovement.Instance != null && PlayerMovement.Instance.isSneaking)
        {
            PlayerMovement.Instance.ToggleSneak();
        }

        if (animalInteract != null) 
            animalInteract.ReportMissionOutcome(false);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleNaturalRoam()
    {
        if (animalMover == null) return;

        if (Vector3.Distance(transform.position, currentTarget) < 1.2f) 
            PickNewTarget();

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

    IEnumerator TransitionToFeedingSequence()
    {
        RescueController.Instance.UpdateNoiseMeter(false, Color.green, 1f);

        if (PlayerMovement.Instance != null && PlayerMovement.Instance.isSneaking)
        {
            PlayerMovement.Instance.ToggleSneak();
        }

        yield return new WaitForSeconds(0.5f);

        if (RescueController.Instance.sneakButton != null) 
            RescueController.Instance.sneakButton.SetActive(false);

        if (RescueController.Instance.feedButton != null) 
            RescueController.Instance.feedButton.gameObject.SetActive(true);
    }

    public void OnFeedButtonPressed() 
    {
        if (currentStep == MissionStep.Feeding) 
        {
            RescueController.Instance.feedButton.gameObject.SetActive(false);
            StartCoroutine(PerformFeedingSequence());
        }
    }

    IEnumerator PerformFeedingSequence()
    {
        Animator playerAnim = playerTransform.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.SetTrigger("Interact");

        yield return new WaitForSeconds(0.8f); 

        Vector3 forwardPos = playerTransform.position + (playerTransform.forward * 1.5f);
        Vector3 spawnPos = forwardPos + Vector3.up * 0.3f;

        RaycastHit hit;
        if (Physics.Raycast(forwardPos + Vector3.up, Vector3.down, out hit, 5f))
        {
            spawnPos = hit.point + Vector3.up * 0.2f;
        }

        spawnedFoodBowl = Instantiate(missionData.foodBowlPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = spawnedFoodBowl.GetComponent<Rigidbody>();
        if (rb == null) rb = spawnedFoodBowl.AddComponent<Rigidbody>();

        if (animalInteract != null) 
            animalInteract.SetFoodBowlReference(spawnedFoodBowl);

        yield return new WaitForSeconds(1.2f);

        currentStep = MissionStep.Eating;

        yield return StartCoroutine(AnimalWalkToFood(spawnedFoodBowl.transform.position));

        animalMover.SetInput(Vector2.zero, transform.position, false, false);

        yield return new WaitForSeconds(2.5f);

        CleanupFoodBowl(); // 🔥 SUCCESS CLEANUP

        currentStep = MissionStep.FinishedEating;

        if (PointerController.Instance != null) 
            PointerController.Instance.ShowTamePrompt(animalInteract);
    }

    IEnumerator AnimalWalkToFood(Vector3 targetPos)
    {
        if (animalMover == null) yield break;

        Vector3 stopPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        float timeout = 0;

        while (Vector3.Distance(transform.position, stopPos) > stopDistance && timeout < 7f) 
        {
            float speedScale = Mathf.Clamp01(Vector3.Distance(transform.position, stopPos) / 2f);
            animalMover.SetInput(new Vector2(0, 1.2f * speedScale), stopPos, false, false);
            timeout += Time.deltaTime;
            yield return null;
        }

        animalMover.SetInput(Vector2.zero, transform.position, false, false);
    }

    // 🔥 SINGLE CLEANUP FUNCTION (IMPORTANT)
    private void CleanupFoodBowl()
    {
        if (spawnedFoodBowl != null)
        {
            Destroy(spawnedFoodBowl);
            spawnedFoodBowl = null;
        }
    }
}