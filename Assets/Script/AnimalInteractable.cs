using UnityEngine;
using System.Collections;
using ithappy.Animals_FREE;

public class AnimalInteractable : MonoBehaviour
{
    [Header("Quest")]
    public QuestInfo currentQuest;

    [Header("Run Away Settings")]
    public float runAwaySpeed = 1.2f;
    public float runAwayDuration = 3.5f;
    public float runAwayDistance = 40f;

    [Header("Emote Prefabs")]
    public GameObject angryEmotePrefab;
    public GameObject sneakEnterPrefab;
    public GameObject fullTrustPrefab;

    [Header("Emote Position")]
    public Vector3 emoteOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Emote Rotation Animation")]
    public bool rotateEmote360 = true;
    public float emoteRotateSpeed = 180f; // degrees per second

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
    private bool sneakTriggered = false;
    private bool fullTrustTriggered = false;

    private GameObject activeEmote;

    public void SetupQuest(QuestInfo info)
    {
        currentQuest = info;
        missionLogic = GetComponent<AnimalMissionLogic>();

        SetupCircleRenderer();

        isFailing = false;
        sneakTriggered = false;
        fullTrustTriggered = false;

        if (activeEmote != null)
            Destroy(activeEmote);
    }

    private void SetupCircleRenderer()
    {
        circleRenderer = GetComponent<LineRenderer>();

        if (circleRenderer == null)
            circleRenderer = gameObject.AddComponent<LineRenderer>();

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

    private void SwitchEmote(GameObject prefab)
    {
        if (activeEmote != null)
            Destroy(activeEmote);

        if (prefab != null)
        {
            activeEmote = Instantiate(
                prefab,
                transform.position + emoteOffset,
                Quaternion.identity,
                transform
            );
        }
    }

    void Update()
    {
        RotateActiveEmote();

        if (isFailing || currentQuest == null || missionLogic == null || PlayerMovement.Instance == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            PlayerMovement.Instance.transform.position
        );

        bool inside = distance <= currentQuest.detectionRadius;

        // Sneak enter emote
        if (inside && PlayerMovement.Instance.isSneaking && !sneakTriggered)
        {
            sneakTriggered = true;

            if (sneakEnterPrefab != null && !fullTrustTriggered)
                SwitchEmote(sneakEnterPrefab);
        }

        if (!inside)
            sneakTriggered = false;

        // Detection circle states
        if (missionLogic.currentStep == AnimalMissionLogic.MissionStep.Waiting ||
            missionLogic.currentStep == AnimalMissionLogic.MissionStep.BuildTrust)
        {
            Color circleColor = neutralColor;

            // Fail if player enters not sneaking
            if (inside && !PlayerMovement.Instance.isSneaking)
            {
                ReportMissionOutcome(false);
                return;
            }

            // Trust color transition
            if (inside && PlayerMovement.Instance.isSneaking)
            {
                float max = currentQuest.detectionRadius;
                float min = Mathf.Max(1.5f, max * 0.25f);

                float t = Mathf.InverseLerp(max, min, distance);
                circleColor = Color.Lerp(neutralColor, trustColor, t);
            }

            DrawCircle(currentQuest.detectionRadius, circleColor);
        }
        else
        {
            HideCircle();
        }

        // Full trust emote
        if (missionLogic.currentStep == AnimalMissionLogic.MissionStep.Feeding &&
            !fullTrustTriggered)
        {
            fullTrustTriggered = true;
            SwitchEmote(fullTrustPrefab);
        }
    }

    private void RotateActiveEmote()
    {
        if (activeEmote == null) return;
        if (!rotateEmote360) return;

        activeEmote.transform.Rotate(
            Vector3.up,
            emoteRotateSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private void DrawCircle(float radius, Color color)
    {
        if (circleRenderer == null) return;

        circleRenderer.enabled = true;
        circleRenderer.startColor = color;
        circleRenderer.endColor = color;

        for (int i = 0; i < 51; i++)
        {
            float angle = i * (360f / 50f);

            float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            circleRenderer.SetPosition(
                i,
                new Vector3(x, transform.position.y + circleYOffset, z)
            );
        }
    }

    private void HideCircle()
    {
        if (circleRenderer != null)
            circleRenderer.enabled = false;
    }

    public void ReportMissionOutcome(bool success)
    {
        if (isFailing) return;

        isFailing = !success;

        HideCircle();

        if (!success)
        {
            if (angryEmotePrefab != null)
                SwitchEmote(angryEmotePrefab);

            RescueController.Instance.UpdateNoiseMeter(false, Color.white, 0f);
            RescueController.Instance.ReportMissionOutcome(false);

            StartCoroutine(RunAwayAndVanish());
        }
        else
        {
            if (activeFoodBowl != null)
                Destroy(activeFoodBowl);

            if (activeEmote != null)
                Destroy(activeEmote);

            RescueController.Instance.ReportMissionOutcome(true);
            Destroy(gameObject);
        }
    }

    IEnumerator RunAwayAndVanish()
    {
        if (missionLogic != null)
            missionLogic.enabled = false;

        CreatureMover mover = GetComponent<CreatureMover>();

        if (mover != null)
        {
            Vector3 runDir =
                (transform.position - PlayerMovement.Instance.transform.position).normalized;

            Vector3 target = transform.position + (runDir * runAwayDistance);

            float t = 0f;

            while (t < runAwayDuration)
            {
                mover.SetInput(
                    new Vector2(0, runAwaySpeed),
                    target,
                    true,
                    false
                );

                t += Time.deltaTime;
                yield return null;
            }
        }

        if (activeFoodBowl != null)
            Destroy(activeFoodBowl);

        if (activeEmote != null)
            Destroy(activeEmote);

        Destroy(gameObject);
    }
}