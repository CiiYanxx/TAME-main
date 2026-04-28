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

    [Header("Taming Fine Tuning")]
    public float fullTrustOverrideDistance = 1.5f;

    [Range(0.05f, 0.5f)]
    public float innerTrustRadiusMultiplier = 0.25f;

    public float trustBufferDistance = 2f;


    [Header("Movement Lock On Full Trust")]
    public bool disableMovementAtFullTrust = true;
    public GameObject moveJoystickObject;

    private Vector3 currentTarget;
    private Vector3 spawnPoint;
    private GameObject spawnedFoodBowl;
    private QuestInfo missionData;
    private AnimalInteractable animalInteract;
    private CreatureMover animalMover;

    private float meterValue = 0f;

    private Transform playerTransform;
    private bool missionStarted = false;
    private bool hasTouchedRadius = false;

    
    private bool feedingTriggered = false; // 🔥 IMPORTANT FIX

    public void SetupMission(QuestInfo info)
    {
        missionData = info;

        animalInteract = GetComponent<AnimalInteractable>();
        animalMover = GetComponent<CreatureMover>();

        // Reset mission states
        meterValue = 0f;
        currentStep = MissionStep.Waiting;
        missionStarted = true;

        hasTouchedRadius = false;
        feedingTriggered = false;

        // Clean UI reset first
        if (RescueController.Instance != null)
        {
            RescueController.Instance.HideHint();

            RescueController.Instance.UpdateNoiseMeter(
                false,
                Color.white,
                0f
            );
        }

        // Get player reference
        if (PlayerMovement.Instance != null)
        {
            playerTransform = PlayerMovement.Instance.transform;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerTransform = player.transform;
        }

        // Save spawn point
        spawnPoint = transform.position;

        // Setup animal interaction
        if (animalInteract != null)
            animalInteract.SetupQuest(info);

        // Reset feed button
        if (RescueController.Instance != null &&
            RescueController.Instance.feedButton != null)
        {
            RescueController.Instance.feedButton.gameObject.SetActive(false);

            RescueController.Instance.feedButton.onClick.RemoveAllListeners();

            RescueController.Instance.feedButton.onClick.AddListener(OnFeedButtonPressed);
        }

        // Start roaming
        PickNewTarget();

        // 🔥 SHOW UI ONLY AFTER FULL SETUP
        if (RescueController.Instance != null)
        {
            RescueController.Instance.ShowHint(info.missionHint);

            if (RescueController.Instance.sneakButton != null)
                Debug.Log("<color=cyan>[SNEAK DEBUG]</color> SNEAK BUTTON ON");
                RescueController.Instance.sneakButton.SetActive(true);
        }
    }
    public void OnPlayerInteract()
    {
        if (currentStep == MissionStep.Feeding)
            OnFeedButtonPressed();
    }

    void Update()
    {
        if (!missionStarted || missionData == null || playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float radius = missionData.detectionRadius;

        bool insideRadius = distance <= radius;

        // ✅ ALWAYS control hint here (single source of truth)
        if (RescueController.Instance != null)
        {
            if (insideRadius)
                RescueController.Instance.HideHint();
            else
                RescueController.Instance.ShowHint(missionData.missionHint);
        }

        if (currentStep == MissionStep.Waiting || currentStep == MissionStep.BuildTrust)
        {
            if (distance <= radius * 0.4f && !PlayerMovement.Instance.isSneaking)
            {
                HandleFleeSequence();
                return;
            }

            if (distance <= radius && PlayerMovement.Instance.isSneaking)
            {
                hasTouchedRadius = true;
                currentStep = MissionStep.BuildTrust;

                LookAtPlayer();

                if (animalMover != null)
                    animalMover.SetInput(Vector2.zero, transform.position, false, false);

                if (distance <= fullTrustOverrideDistance)
                {
                    meterValue = 1f;
                }
                else
                {
                    float minDist = radius * innerTrustRadiusMultiplier;

                    float t = Mathf.InverseLerp(radius, minDist, distance);
                    float targetValue = Mathf.Clamp01(t);

                    float speedMultiplier = Mathf.Lerp(0.6f, 2.5f, targetValue);

                    meterValue = Mathf.MoveTowards(
                        meterValue,
                        targetValue,
                        Time.deltaTime * speedMultiplier
                    );
                }
            }
            else
            {
                HandleNaturalRoam();

                if (hasTouchedRadius)
                {
                    meterValue -= Time.deltaTime * missionData.drainSpeed;
                    meterValue = Mathf.Clamp01(meterValue);

                    if (meterValue <= 0f)
                    {
                        HandleFleeSequence();
                        return;
                    }
                }
            }

            if (hasTouchedRadius)
            {
                Color stateColor;

                if (meterValue < 0.35f)
                    stateColor = Color.red;
                else
                {
                    float t = Mathf.InverseLerp(0.35f, 1f, meterValue);
                    stateColor = Color.Lerp(Color.yellow, Color.green, t);
                }

                RescueController.Instance.UpdateNoiseMeter(true, stateColor, meterValue);
            }

            if (meterValue >= 1f && !feedingTriggered)
            {
                feedingTriggered = true;
                currentStep = MissionStep.Feeding;

                if (RescueController.Instance != null)
                    RescueController.Instance.OnFullTrustReached();

                if (TutorialController.Instance != null)
                    TutorialController.Instance.Tutorial6_Feed();

                StartCoroutine(TransitionToFeedingSequence());
            }
        }
    }

    private void HandleFleeSequence()
    {
        missionStarted = false;

        if (RescueController.Instance != null)
            RescueController.Instance.HideHint();

        CleanupFoodBowl();

        if (PlayerMovement.Instance != null && PlayerMovement.Instance.isSneaking)
            PlayerMovement.Instance.ToggleSneak();

        if (animalInteract != null)
            animalInteract.ReportMissionOutcome(false);
    }
    private void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private void HandleNaturalRoam()
    {
        if (animalMover == null) return;

        if (Vector3.Distance(transform.position, currentTarget) < 1.2f)
            PickNewTarget();

        Vector3 direction = (currentTarget - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
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
            PlayerMovement.Instance.ToggleSneak();

        yield return new WaitForSeconds(0.5f);

        if (RescueController.Instance.sneakButton != null)
            Debug.Log("<color=red>[SNEAK DEBUG]</color> SNEAK BUTTON OFF");
            RescueController.Instance.sneakButton.SetActive(false);

        if (RescueController.Instance.feedButton != null)
        RescueController.Instance.feedButton.gameObject.SetActive(true);

        if (PlayerMovement.Instance != null)
        PlayerMovement.Instance.canControl = true;
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

        if (playerAnim != null)
            playerAnim.SetTrigger("Interact");

        yield return new WaitForSeconds(0.8f);

        Vector3 forwardPos = playerTransform.position + (playerTransform.forward * 1.5f);
        Vector3 spawnPos = forwardPos + Vector3.up * 0.3f;

        RaycastHit hit;

        if (Physics.Raycast(forwardPos + Vector3.up, Vector3.down, out hit, 5f))
            spawnPos = hit.point + Vector3.up * 0.2f;

        spawnedFoodBowl = Instantiate(missionData.foodBowlPrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = spawnedFoodBowl.GetComponent<Rigidbody>();
        if (rb == null)
            rb = spawnedFoodBowl.AddComponent<Rigidbody>();

        if (animalInteract != null)
            animalInteract.SetFoodBowlReference(spawnedFoodBowl);

        yield return new WaitForSeconds(1.2f);

        currentStep = MissionStep.Eating;

        yield return StartCoroutine(AnimalWalkToFood(spawnedFoodBowl.transform.position));

        animalMover.SetInput(Vector2.zero, transform.position, false, false);

        yield return new WaitForSeconds(2.5f);

        CleanupFoodBowl();

        currentStep = MissionStep.FinishedEating;

        if (PointerController.Instance != null)
            PointerController.Instance.ShowTamePrompt(animalInteract);
    }

    IEnumerator AnimalWalkToFood(Vector3 targetPos)
    {
        if (animalMover == null) yield break;

        Vector3 stopPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);

        float timeout = 0f;

        while (Vector3.Distance(transform.position, stopPos) > stopDistance && timeout < 7f)
        {
            float speedScale = Mathf.Clamp01(Vector3.Distance(transform.position, stopPos) / 2f);

            animalMover.SetInput(new Vector2(0, 1.2f * speedScale), stopPos, false, false);

            timeout += Time.deltaTime;
            yield return null;
        }

        animalMover.SetInput(Vector2.zero, transform.position, false, false);
    }

    private void CleanupFoodBowl()
    {
        if (spawnedFoodBowl != null)
        {
            Destroy(spawnedFoodBowl);
            spawnedFoodBowl = null;
        }
    }
}