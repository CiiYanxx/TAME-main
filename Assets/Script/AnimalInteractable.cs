using UnityEngine;
using System.Collections;
using ithappy.Animals_FREE;

public class AnimalInteractable : MonoBehaviour
{
    public QuestInfo currentQuest; 
    public float runAwaySpeed = 1.2f;
    public float runAwayDuration = 3.5f;
    public GameObject angryEmotePrefab;
    public Vector3 emoteOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Circle Visual Settings")]
    public float circleYOffset = 0.1f; 
    public float circleWidth = 0.15f;
    
    // Adjustable sa Inspector para sa transparency
    public Color neutralColor = new Color(1f, 0.92f, 0.016f, 0.5f); // Yellow Transparent
    public Color alertColor = new Color(1f, 0f, 0f, 0.5f);          // Red Transparent
    public Color trustColor = new Color(0f, 1f, 0f, 0.5f);          // Green Transparent

    private LineRenderer circleRenderer;
    private GameObject activeFoodBowl;
    private AnimalMissionLogic missionLogic;
    private bool isFailing = false;

    public void SetupQuest(QuestInfo info) { 
        currentQuest = info; 
        missionLogic = GetComponent<AnimalMissionLogic>();
        SetupCircleRenderer();
    }

    private void SetupCircleRenderer() {
        circleRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
        
        // Sprites/Default shader para gumana ang Alpha/Transparency
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        circleRenderer.useWorldSpace = true;
        circleRenderer.loop = true;
        circleRenderer.positionCount = 51;
        circleRenderer.startWidth = circleWidth; 
        circleRenderer.endWidth = circleWidth;
        circleRenderer.sortingOrder = 5;
        circleRenderer.enabled = false;
    }

    public void SetFoodBowlReference(GameObject bowl) { 
        activeFoodBowl = bowl; 
    }

    void Update() {
        // Stop update kung nag-f-fail na (running shrink coroutine)
        if (isFailing || currentQuest == null || missionLogic == null || PlayerMovement.Instance == null) return;

        float distance = Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position);

        // Check steps para sa visual feedback
        if (missionLogic.currentStep == AnimalMissionLogic.MissionStep.BuildTrust || missionLogic.currentStep == AnimalMissionLogic.MissionStep.Waiting) {
            HandleSingleCircleLogic(distance);
        } else {
            // Pag Feeding/Eating na, off ang visuals
            if (circleRenderer != null && circleRenderer.enabled) {
                circleRenderer.enabled = false;
                RescueController.Instance.UpdateNoiseMeter(false, Color.white, 0f);
            }
        }
    }

    private void HandleSingleCircleLogic(float distance) {
        Color targetColor = neutralColor;
        float currentMeterValue = missionLogic.GetTrustPercentage();

        // Visual feedback based on player movement
        if (PlayerMovement.Instance.isRunning) {
            targetColor = alertColor;
        } else if (PlayerMovement.Instance.isSneaking) {
            // Green kung mataas na ang meter
            if (currentMeterValue > 0.8f) {
                targetColor = trustColor;
            } else {
                targetColor = neutralColor;
            }
        }

        // Update ang UI Meter line sa RescueController
        // Ipinapasa ang isVisible, color, at yung float value (0-1)
        if (distance <= currentQuest.detectionRadius) {
            RescueController.Instance.UpdateNoiseMeter(true, targetColor, currentMeterValue);
        } else {
            // Pag lumabas sa radius, hayaan ang AnimalMissionLogic ang mag-drain
            // Pero kailangan pa rin ipakita ang meter hanggang mag-0
            RescueController.Instance.UpdateNoiseMeter(true, targetColor, currentMeterValue);
        }

        DrawCircle(currentQuest.detectionRadius, targetColor);
    }

    private void DrawCircle(float radius, Color color) {
        if (circleRenderer == null) return;
        
        circleRenderer.enabled = true;
        circleRenderer.startColor = color;
        circleRenderer.endColor = color;

        for (int i = 0; i < 51; i++) {
            float angle = i * (360f / 50);
            float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            circleRenderer.SetPosition(i, new Vector3(x, transform.position.y + circleYOffset, z));
        }
    }

    public void ReportMissionOutcome(bool success) {
        if (success) {
            if (activeFoodBowl != null) Destroy(activeFoodBowl);
            RescueController.Instance.ReportMissionOutcome(true);
            Destroy(gameObject);
        } else {
            isFailing = true; // Stop Update() logic
            StartCoroutine(ShrinkCircleAndRun());
        }
    }

    IEnumerator ShrinkCircleAndRun() {
        float startRadius = currentQuest.detectionRadius;
        float t = 0;
        float duration = 0.4f; // Gaano kabilis liliit yung circle

        if (angryEmotePrefab != null) {
            Instantiate(angryEmotePrefab, transform.position + emoteOffset, Quaternion.identity, transform);
        }

        while (t < 1.0f) {
            t += Time.deltaTime / duration;
            float currentR = Mathf.Lerp(startRadius, 0.01f, t);
            DrawCircle(currentR, alertColor); 
            yield return null;
        }

        if(circleRenderer != null) circleRenderer.enabled = false;
        
        // Patayin ang noise meter UI
        RescueController.Instance.UpdateNoiseMeter(false, Color.white, 0f);
        RescueController.Instance.ReportMissionOutcome(false);
        
        yield return StartCoroutine(RunAwayAndVanish());
    }

    IEnumerator RunAwayAndVanish() {
        if(missionLogic != null) missionLogic.enabled = false;
        
        CreatureMover mover = GetComponent<CreatureMover>();
        if (mover != null) {
            Vector3 runDir = (transform.position - PlayerMovement.Instance.transform.position).normalized;
            Vector3 target = transform.position + (runDir * 20f);
            float t = 0;
            while (t < runAwayDuration) {
                mover.SetInput(new Vector2(0, 1 * runAwaySpeed), target, true, false);
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        if (activeFoodBowl != null) Destroy(activeFoodBowl);
        Destroy(gameObject);
    }
}