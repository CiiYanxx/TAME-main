using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Kailangan para sa Coroutine

public class NPC : MonoBehaviour
{
    public static NPC Instance { get; private set; }

    [Header("Quest Data")]
    public List<QuestInfo> allQuests;
    private List<Quest> quests = new List<Quest>();

    [Header("Status")]
    public int totalCompletedMissions = 0;
    public bool playerInRange, isTalkingWithPlayer;

    [Header("Distance Settings")]
    public float interactionDistance = 3.5f;
    private float nextCheckTime;
    private float checkInterval = 0.2f;

    private int introStep = 0;
    private string coloredPlayerName = "Player";
    private bool missionReturned = false;
    private bool lastMissionWasSuccess = false;

    public static Action OnQuestStateChanged;

    private Dictionary<string, float> missionCooldowns = new Dictionary<string, float>();
    private Dictionary<string, int> missionFailCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        foreach (QuestInfo info in allQuests)
            quests.Add(new Quest(info));
    }

    private void Start()
    {
        // 1. Load Player Name
        string savedName = PlayerPrefs.GetString("Character_Name", "");
        if (string.IsNullOrEmpty(savedName)) savedName = "Rescue Hero";
        coloredPlayerName = $"<color=#00FFFF>{savedName}</color>";

        // 2. Load Save Data and Log Status
        GameData data = SaveSystem.Load();
        if (data != null)
        {
            totalCompletedMissions = data.completedMissions;
            Debug.Log($"<color=green>[NPC GAMELOG]</color> Data Loaded. Completed Missions: {totalCompletedMissions}");

            if (PlayerPrefs.GetInt("IsMissionActive", 0) == 1)
            {
                int locIdx = PlayerPrefs.GetInt("ActiveLocIdx", -1);
                int missIdx = PlayerPrefs.GetInt("ActiveMissIdx", -1);
                Debug.Log($"<color=cyan>[NPC GAMELOG]</color> Active Mission Found: Location {locIdx}, Mission {missIdx}");

                QuestInfo activeInfo = allQuests.Find(q => q.locationIndex == locIdx && q.missionIndex == missIdx);
                if (activeInfo != null)
                {
                    Quest q = quests.Find(quest => quest.info == activeInfo);
                    if (q != null) q.accepted = true;

                    if (RescueController.Instance != null)
                        RescueController.Instance.StartMission(this, activeInfo);
                }
            }
        }
        else
        {
            Debug.Log("<color=red>[NPC GAMELOG]</color> No save data found. System ready for new game.");
        }

        // 3. Simulan ang Auto-Save loop tuwing 5 seconds
        StartCoroutine(AutoSaveLoop());
    }

    IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f); 
            SaveProgress();
        }
    }

    public void ResumeWithLoading(string loadingSceneName)
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(loadingSceneName);
    }

    public void SetMissionReturned(bool state, bool success)
    {
        missionReturned = state;
        lastMissionWasSuccess = success;
        Debug.Log($"<color=yellow>[NPC GAMELOG]</color> Mission Returned. Success: {success}");
    }

    public void ReportQuestOutcome(bool success)
    {
        Quest active = quests.Find(q => q.accepted && !q.isCompleted);

        if (active != null)
        {
            string missionID = active.info.targetAnimalName;

            if (success)
            {
                active.isCompleted = true;
                totalCompletedMissions++;
                PlayerPrefs.SetInt("Mission_" + missionID, 1);

                Debug.Log($"<color=green>[NPC GAMELOG]</color> Mission Success! Total: {totalCompletedMissions}");

                if (missionFailCounts.ContainsKey(missionID))
                    missionFailCounts[missionID] = 0;
            }
            else
            {
                active.accepted = false;

                if (!missionFailCounts.ContainsKey(missionID))
                    missionFailCounts[missionID] = 0;

                missionFailCounts[missionID]++;

                float baseCooldown = 60f;
                float totalPenalty = baseCooldown * missionFailCounts[missionID];

                missionCooldowns[missionID] = Time.time + totalPenalty;

                Debug.Log($"<color=red>[NPC GAMELOG]</color> Mission Failed. Cooldown: {totalPenalty}s");
            }

            PlayerPrefs.SetInt("IsMissionActive", 0);

            SaveProgress();

            // 🔥 AUTO REFRESH LOCATION LOCKS
            if (DialogSystem.Instance != null)
                DialogSystem.Instance.RefreshLocks();

            OnQuestStateChanged?.Invoke();
        }
    }

    public void SaveProgress()
    {
        string rawName = PlayerPrefs.GetString("Character_Name", "Rescue Hero")
            .Replace("<color=#00FFFF>", "").Replace("</color>", "");

        Vector3 pos = PlayerMovement.Instance != null ? PlayerMovement.Instance.transform.position : Vector3.zero;
        int points = RescuePointsHandler.Instance != null ? RescuePointsHandler.Instance.currentPoints : 0;

        // I-save gamit ang SaveSystem
        SaveSystem.Save(totalCompletedMissions, points, pos, rawName, "");
        PlayerPrefs.Save();
        // Debug.Log("<color=grey>[NPC GAMELOG]</color> Auto-save triggered.");
    }

    void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckInteractionDistance();
        }
    }

    private void CheckInteractionDistance()
    {
        if (PlayerMovement.Instance == null) return;

        float sqrDist = (transform.position - PlayerMovement.Instance.transform.position).sqrMagnitude;
        float sqrLimit = interactionDistance * interactionDistance;

        if (sqrDist <= sqrLimit)
        {
            playerInRange = true;
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                if (isTalkingWithPlayer) EndConversation();
            }
        }
    }

    public void StartConversation()
    {
        isTalkingWithPlayer = true;
        Debug.Log("<color=white>[NPC GAMELOG]</color> Conversation started.");

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = false;

        if (CameraSystem.Instance != null)
            CameraSystem.Instance.EnableConversationMode(true);

        DialogSystem.Instance.OpenDialogUI();

        Quest active = quests.Find(q => q.accepted && !q.isCompleted);

        // FLOW CHECK: MISSION RETURNED
        if (missionReturned)
        {
            DialogSystem.Instance.SetDialogText(lastMissionWasSuccess
            ? $"<color=#2B7B98>Dr. Kevin:</color> Hey {coloredPlayerName}, you did it! I'll start assessing it now and check its condition carefully. Would you like to rescue more?"
            : $"<color=#2B7B98>Dr. Kevin:</color> Hmm… that didn’t go well. Want to try again?");

            SetupOption1("Continue", () =>
            {
                missionReturned = false;
                ShowLocationSelection();
            });
            return;
        }

        // FLOW CHECK: CURRENTLY ON MISSION
        if (active != null)
        {
            DialogSystem.Instance.SetDialogText($"<color=#2B7B98>Dr. Kevin:</color> Hey {coloredPlayerName}, you're still on a rescue mission. Please be careful.");
            SetupOption1("I'm on it!", EndConversation);
            return;
        }

        // FLOW CHECK: INTRO VS REGULAR DIALOG
        // FIX: Kung may completed missions na, i-skip ang intro tutorial steps
        if (totalCompletedMissions == 0 && introStep < 4)
        {
            Debug.Log("<color=orange>[NPC GAMELOG]</color> Showing Intro Sequence.");
            ShowIntroSequence();
        }
        else
        {
            Debug.Log("<color=orange>[NPC GAMELOG]</color> Intro skipped (Missions already done).");
            DialogSystem.Instance.SetDialogText($"<color=#2B7B98>Dr. Kevin:</color> Good day {coloredPlayerName}! Are you ready for a new rescue mission?");
            SetupOption1("Continue", ShowLocationSelection);
        }
    }

    private void ShowIntroSequence()
    {
        DialogSystem.Instance.option2BTN.gameObject.SetActive(false);

        switch (introStep)
        {
            case 0:
                DialogSystem.Instance.SetDialogText("Hi, welcome to Stray Ville. I'm <color=#2B7B98>Dr. Kevin</color>, your Local Veterinarian here.");
                break;

            case 1:
                DialogSystem.Instance.SetDialogText($"Hello, I'm {coloredPlayerName}. Nice to meet you.");
                break;

            case 2:
                DialogSystem.Instance.SetDialogText("<color=#2B7B98>Dr. Kevin:</color> Good. We really need someone like you right now.");
                break;

            case 3:
                DialogSystem.Instance.SetDialogText("<color=#2B7B98>Dr. Kevin:</color> Let's start with your first rescue mission. Choose area to rescue.");
                break;
        }

        SetupOption1("Next", () => 
        { 
            introStep++; 
            if (introStep >= 4) ShowLocationSelection();
            else ShowIntroSequence(); 
        });
    }

    public void AcceptMission(QuestInfo info)
    {
        Quest q = quests.Find(quest => quest.info == info);
        if (q != null) q.accepted = true;

        PlayerPrefs.SetInt("IsMissionActive", 1);
        PlayerPrefs.SetInt("ActiveLocIdx", info.locationIndex);
        PlayerPrefs.SetInt("ActiveMissIdx", info.missionIndex);
        
        SaveProgress();
        Debug.Log($"<color=cyan>[NPC GAMELOG]</color> Mission Accepted: {info.targetAnimalName}");

        DialogSystem.Instance.OpenDialogUI();
        DialogSystem.Instance.SetDialogText("<color=#2B7B98>Dr. Kevin:</color> Good luck! Stay safe out there");

        SetupOption1("Continue", () =>
        {
            if (RescueController.Instance != null)
                RescueController.Instance.StartMission(this, info);
            EndConversation();
        });
    }

    public void ShowLocationSelection()
    {
        Debug.Log("<color=magenta>[NPC GAMELOG]</color> Showing Location Selection UI.");
        DialogSystem.Instance.OpenLocationSelection();
    }

    private void SetupOption1(string text, UnityEngine.Events.UnityAction action)
    {
        DialogSystem.Instance.option1BTN.GetComponentInChildren<TextMeshProUGUI>().text = text;
        DialogSystem.Instance.option1BTN.onClick.RemoveAllListeners();
        DialogSystem.Instance.option1BTN.onClick.AddListener(action);
        DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
    }

    public void EndConversation()
    {
        isTalkingWithPlayer = false;
        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = true;

        if (CameraSystem.Instance != null)
            CameraSystem.Instance.EnableConversationMode(false);

        DialogSystem.Instance.CloseAllPanels();
        SaveProgress();
        Debug.Log("<color=white>[NPC GAMELOG]</color> Conversation ended.");

        if (TutorialController.Instance != null)
        {
            TutorialController.Instance.OnConversationEnd();
        }

        if (TutorialController.Instance != null)
        {
            TutorialController.Instance.OnConversationEnd();
        }

        Debug.Log("<color=red>[NPC]</color> Ending conversation → triggering arrow cleanup");
    }

    public bool HasCooldown(string missionID)
    {
        if (!missionCooldowns.ContainsKey(missionID)) return false;

        return Time.time < missionCooldowns[missionID];
    }

    public float GetCooldownRemaining(string missionID)
    {
        if (!missionCooldowns.ContainsKey(missionID)) return 0f;

        return Mathf.Max(0, missionCooldowns[missionID] - Time.time);
    }

    public void ShowExitDialogue()
    {
        if (DialogSystem.Instance == null) return;

        DialogSystem.Instance.OpenDialogUI();

        DialogSystem.Instance.SetDialogText(
            $"<color=#2B7B98>Dr. Kevin:</color> You've done enough for now, {coloredPlayerName}. Take a rest and come back when you're ready.");

        SetupOption1("Leave", EndConversation);
    }
}