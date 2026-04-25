using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCard : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject acceptButtonObj;    
    public GameObject successImageObj;    
    public GameObject timerButtonObj;     
    public TextMeshProUGUI timerText;     

    [Header("Display Info")]
    public Image animalIcon;
    public TextMeshProUGUI titleText;

    private QuestInfo currentInfo;
    private NPC currentNpc;

    private float cooldownTimer = 0f;
    private bool isCooldown = false;

    private static Transform activeArrowRef;

    public void Setup(QuestInfo info, NPC npc, bool canAccept)
    {
        currentInfo = info;
        currentNpc = npc;

        if (titleText != null)
            titleText.text = info.questTitle;

        if (animalIcon != null && info.animalIcon != null)
            animalIcon.sprite = info.animalIcon;

        if (acceptButtonObj != null)
        {
            Button btn = acceptButtonObj.GetComponent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    DialogSystem.Instance.OpenMissionPreview(currentInfo, currentNpc);
                });
            }
        }

        CheckCooldownFromNPC();
        RefreshUI();
    }

    void Update()
    {
        if (!isCooldown) return;

        cooldownTimer -= Time.deltaTime;

        if (timerText != null)
            timerText.text = FormatTime(cooldownTimer);

        if (cooldownTimer <= 0)
        {
            isCooldown = false;
            RefreshUI();
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        timeInSeconds = Mathf.Max(0, timeInSeconds);

        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return minutes > 0 ? $"{minutes}m {seconds:00}s" : $"{seconds}s";
    }

    Transform arrowRef;


    public static void DisableActiveQuestArrow()
    {
        if (activeArrowRef != null)
        {
            activeArrowRef.gameObject.SetActive(false);
            Debug.Log("[QuestCard] FORCE DISABLE Prefab_Arrow → " + activeArrowRef.name);

            if (TutorialController.Instance != null)
            {
                TutorialController.Instance.UnregisterRuntimeArrow(activeArrowRef.gameObject);
            }

            activeArrowRef = null;
        }
    }

    public void DisableArrow()
    {
        Transform arrow = transform.Find("Prefab_Arrow");

        if (arrow != null)
        {
            arrow.gameObject.SetActive(false);
            Debug.Log("[QuestCard] FORCE DISABLE Prefab_Arrow → " + arrow.name);
        }
    }

    public void RefreshUI()
    {
        if (currentInfo == null || currentNpc == null) return;

        string missionID = currentInfo.targetAnimalName;

        bool isFinished = PlayerPrefs.GetInt("Mission_" + missionID, 0) == 1;
        bool hasCooldown = currentNpc.HasCooldown(missionID);

        acceptButtonObj.SetActive(false);
        successImageObj.SetActive(false);
        timerButtonObj.SetActive(false);

        if (isFinished)
        {
            successImageObj.SetActive(true);
        }
        else if (hasCooldown)
        {
            timerButtonObj.SetActive(true);

            cooldownTimer = currentNpc.GetCooldownRemaining(missionID);
            isCooldown = true;

            Button btn = timerButtonObj.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
        else
        {
            acceptButtonObj.SetActive(true);
        }
    }

    void CheckCooldownFromNPC()
    {
        if (currentNpc == null || currentInfo == null) return;

        string missionID = currentInfo.targetAnimalName;

        if (currentNpc.HasCooldown(missionID))
        {
            float remaining = currentNpc.GetCooldownRemaining(missionID);

            if (remaining > 0)
            {
                cooldownTimer = remaining;
                isCooldown = true;
            }
        }
    }

    public void StartCooldown(float seconds)
    {
        cooldownTimer = seconds;
        isCooldown = true;
        RefreshUI();
    }
}