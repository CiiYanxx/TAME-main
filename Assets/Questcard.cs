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

    // =========================
    // SETUP (FIXED)
    // =========================
    public void Setup(QuestInfo info, NPC npc, bool canAccept)
    {
        currentInfo = info;
        currentNpc = npc;

        // 🔥 SAFE UI SET
        if (titleText != null)
            titleText.text = info.questTitle;

        if (animalIcon != null && info.animalIcon != null)
            animalIcon.sprite = info.animalIcon;

        // 🔥 BUTTON SETUP
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

        // 🔥 CHECK COOLDOWN FROM NPC (IMPORTANT FIX)
        CheckCooldownFromNPC();

        RefreshUI();
    }

    // =========================
    // UPDATE TIMER
    // =========================
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

    // =========================
    // FORMAT TIMER
    // =========================
    private string FormatTime(float timeInSeconds)
    {
        timeInSeconds = Mathf.Max(0, timeInSeconds);

        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        if (minutes > 0)
            return $"{minutes}m {seconds:00}s";
        else
            return $"{seconds}s";
    }

    
    public void RefreshUI()
    {
        if (currentInfo == null || currentNpc == null) return;

        string missionID = currentInfo.targetAnimalName;

        bool isFinished = PlayerPrefs.GetInt("Mission_" + missionID, 0) == 1;
        bool hasCooldown = currentNpc.HasCooldown(missionID);

        // RESET ALL
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

    // =========================
    // 🔥 SYNC COOLDOWN WITH NPC
    // =========================
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

    private void OnEnable()
    {
        NPC.OnQuestStateChanged += RefreshUI;
    }

    private void OnDisable()
    {
        NPC.OnQuestStateChanged -= RefreshUI;
    }
    public void StartCooldown(float seconds)
    {
        cooldownTimer = seconds;
        isCooldown = true;
        RefreshUI();
    }
}