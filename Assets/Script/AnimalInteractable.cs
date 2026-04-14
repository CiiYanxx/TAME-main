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
    
    public Color neutralColor = new Color(1f, 0.92f, 0.016f, 0.5f); 
    public Color alertColor = new Color(1f, 0f, 0f, 0.5f);          
    public Color trustColor = new Color(0f, 1f, 0f, 0.5f);          

    private LineRenderer circleRenderer;
    private GameObject activeFoodBowl;
    private AnimalMissionLogic missionLogic;
    private bool isFailing = false;

    public void SetupQuest(QuestInfo info) 
    { 
        currentQuest = info; 
        missionLogic = GetComponent<AnimalMissionLogic>();
        SetupCircleRenderer();
    }

    private void SetupCircleRenderer() 
    {
        circleRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.useWorldSpace = true;
        circleRenderer.loop = true;
        circleRenderer.positionCount = 51;
        circleRenderer.startWidth = circleWidth; 
        circleRenderer.endWidth = circleWidth;
        circleRenderer.sortingOrder = 5;
        circleRenderer.enabled = false;
    }

    public void SetFoodBowlReference(GameObject bowl) 
    { 
        activeFoodBowl = bowl; 
    }

    void Update() 
    {
        if (isFailing || currentQuest == null || missionLogic == null || PlayerMovement.Instance == null) return;

        // ✅ SHOW circle only during WAITING / BUILD TRUST
        if (missionLogic.currentStep == AnimalMissionLogic.MissionStep.Waiting || 
            missionLogic.currentStep == AnimalMissionLogic.MissionStep.BuildTrust)
        {
            DrawCircle(
                currentQuest.detectionRadius, 
                PlayerMovement.Instance.isSneaking ? neutralColor : alertColor
            );
        }
        else
        {
            // ✅ HIDE kapag Feeding / Eating / Finished
            HideCircle();
        }
    }

    private void DrawCircle(float radius, Color color) 
    {
        if (circleRenderer == null) return;
        
        circleRenderer.enabled = true;
        circleRenderer.startColor = color;
        circleRenderer.endColor = color;

        for (int i = 0; i < 51; i++) 
        {
            float angle = i * (360f / 50);
            float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            circleRenderer.SetPosition(i, new Vector3(x, transform.position.y + circleYOffset, z));
        }
    }

    // ✅ NEW: clean hide function
    private void HideCircle()
    {
        if (circleRenderer != null && circleRenderer.enabled)
        {
            circleRenderer.enabled = false;
        }
    }

    public void ReportMissionOutcome(bool success) 
    {
        if (success) 
        {
            if (activeFoodBowl != null) Destroy(activeFoodBowl);
            RescueController.Instance.ReportMissionOutcome(true);
            Destroy(gameObject);
        } 
        else 
        {
            if (!isFailing)
            {
                isFailing = true;

                // 🔴 INSTANT REMOVE CIRCLE (fix sa pangit na shrink)
                HideCircle();

                // optional angry emote
                if (angryEmotePrefab != null) 
                {
                    Instantiate(angryEmotePrefab, transform.position + emoteOffset, Quaternion.identity, transform);
                }

                // reset UI
                RescueController.Instance.UpdateNoiseMeter(false, Color.white, 0f);
                RescueController.Instance.ReportMissionOutcome(false);

                // run away
                StartCoroutine(RunAwayAndVanish());
            }
        }
    }

    IEnumerator RunAwayAndVanish() 
    {
        if(missionLogic != null) missionLogic.enabled = false;
        
        CreatureMover mover = GetComponent<CreatureMover>();
        if (mover != null) 
        {
            Vector3 runDir = (transform.position - PlayerMovement.Instance.transform.position).normalized;
            Vector3 target = transform.position + (runDir * 20f);
            float t = 0;

            while (t < runAwayDuration) 
            {
                mover.SetInput(new Vector2(0, 1 * runAwaySpeed), target, true, false);
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        if (activeFoodBowl != null) Destroy(activeFoodBowl);
        Destroy(gameObject);
    }
}