using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; 
using UnityEngine;
using UnityEngine.UI;

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
    // OPTIMIZATION: Check interval para iwas lag
    private float nextCheckTime;
    private float checkInterval = 0.2f; 

    private int introStep = 0;
    private string coloredPlayerName = "Player";
    private bool missionReturned = false;
    private bool lastMissionWasSuccess = false;

    private void Awake() 
    { 
        if (Instance == null) Instance = this;
        foreach (QuestInfo info in allQuests) quests.Add(new Quest(info)); 
    }

    private void Start()
    {
        string savedName = PlayerPrefs.GetString("Character_Name", "");
        if (string.IsNullOrEmpty(savedName)) savedName = "Rescue Hero"; 

        coloredPlayerName = $"<color=#00FFFF>{savedName}</color>";

        GameData data = SaveSystem.Load();
        if (data != null)
        {
            totalCompletedMissions = data.completedMissions;

            if (PlayerPrefs.GetInt("IsMissionActive", 0) == 1)
            {
                int locIdx = PlayerPrefs.GetInt("ActiveLocIdx", -1);
                int missIdx = PlayerPrefs.GetInt("ActiveMissIdx", -1);

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
    }

    public void ReportQuestOutcome(bool success)
    {
        Quest active = quests.Find(q => q.accepted && !q.isCompleted);
        if (active != null) 
        {
            if (success) 
            { 
                active.isCompleted = true; 
                totalCompletedMissions++;
            }
            else 
            {
                active.accepted = false; 
            }

            PlayerPrefs.SetInt("IsMissionActive", 0);
            SaveProgress();
        }
    }

    private void SaveProgress()
    {
        string rawName = PlayerPrefs.GetString("Character_Name", "Rescue Hero")
                        .Replace("<color=#00FFFF>", "").Replace("</color>", "");
        
        Vector3 pos = PlayerMovement.Instance != null ? PlayerMovement.Instance.transform.position : Vector3.zero;
        int points = RescuePointsHandler.Instance != null ? RescuePointsHandler.Instance.currentPoints : 0;
        
        SaveSystem.Save(totalCompletedMissions, points, pos, rawName, "");
        PlayerPrefs.Save();
    }

    void Update()
    {
        // OPTIMIZATION: Imbes na bawat frame, i-check lang distansya bawat 0.2 seconds
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckInteractionDistance();
        }
    }

    private void CheckInteractionDistance()
    {
        if (PlayerMovement.Instance == null) return;

        // Gamit ang sqrMagnitude (mas mabilis kaysa Vector3.Distance)
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
        if(PlayerMovement.Instance != null) PlayerMovement.Instance.canControl = false;

        if (CameraSystem.Instance != null) CameraSystem.Instance.EnableConversationMode(true);

        DialogSystem.Instance.OpenDialogUI();
        Quest active = quests.Find(q => q.accepted && !q.isCompleted);

        if (missionReturned)
        {
            if (lastMissionWasSuccess)
            {
                DialogSystem.Instance.dialogText.text = $"Kevin: Thankyou verymuch you rescue the animals, Would you like to rescue more?";
            }
            else
            {
                DialogSystem.Instance.dialogText.text = $"Kevin: Oh no! you seems to fail your mission, would you like to restart it?";
            }

            SetupOption1("Continue", () => {
                missionReturned = false;
                ShowLocationSelection();
            });
            DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
            return;
        }

        if (active != null) 
        {
            DialogSystem.Instance.dialogText.text = $"Kevin: {coloredPlayerName} the Stray Animals is still out there, please take care of it.";
            SetupOption1("I'm on it!", EndConversation);
            DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
            return;
        }

        if (totalCompletedMissions == 0 && introStep < 4) 
        {
            ShowIntroSequence();
        }
        else 
        {
            DialogSystem.Instance.dialogText.text = $"Kevin: Hello {coloredPlayerName}! Ready for a new rescue mission?";
            SetupOption1("Continue", ShowLocationSelection);
            DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
        }
    }

    private void ShowIntroSequence()
    {
        DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
        switch (introStep)
        {
            case 0:
                DialogSystem.Instance.dialogText.text = "Hi welcome to Stray Ville my name is Kevin and i am the Local Veterinarian here";
                SetupOption1("Next", () => { introStep++; ShowIntroSequence(); });
                break;
            case 1:
                DialogSystem.Instance.dialogText.text = $"You: Hello my name is {coloredPlayerName} and I am here to help rescue the animals.";
                SetupOption1("Next", () => { introStep++; ShowIntroSequence(); });
                break;
            case 2:
                DialogSystem.Instance.dialogText.text = "Kevin: That's great! We really need someone like you to keep our furry friends safe.";
                SetupOption1("Next", () => { introStep++; ShowIntroSequence(); });
                break;
            case 3:
                DialogSystem.Instance.dialogText.text = "Kevin: Would you like to pick the area were you rescuing?";
                SetupOption1("Next", () => { introStep = 4; ShowLocationSelection(); });
                break;
        }
    }

    public void AcceptMission(QuestInfo info)
    {
        Quest q = quests.Find(quest => quest.info == info);
        if (q != null) q.accepted = true;

        PlayerPrefs.SetInt("IsMissionActive", 1);
        PlayerPrefs.SetInt("ActiveLocIdx", info.locationIndex);
        PlayerPrefs.SetInt("ActiveMissIdx", info.missionIndex);
        SaveProgress();

        DialogSystem.Instance.OpenDialogUI(); 
        DialogSystem.Instance.dialogText.text = "Kevin: Good luck out there and please rescue the animals";
        
        SetupOption1("Continue", () => {
            if(RescueController.Instance != null) RescueController.Instance.StartMission(this, info); 
            EndConversation();
        });
        DialogSystem.Instance.option2BTN.gameObject.SetActive(false);
    }

    public void ShowLocationSelection()
    {
        DialogSystem.Instance.OpenLocationSelection();
        for (int i = 0; i < DialogSystem.Instance.locationCards.Length; i++) 
        {
            int idx = i;
            bool isUnlocked = (totalCompletedMissions >= idx * 10);
            
            DialogSystem.Instance.locationCards[idx].interactable = isUnlocked;

            if (i < DialogSystem.Instance.locationLocks.Length && DialogSystem.Instance.locationLocks[i] != null)
            {
                DialogSystem.Instance.locationLocks[i].SetActive(!isUnlocked);
            }

            DialogSystem.Instance.locationCards[idx].onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                DialogSystem.Instance.locationCards[idx].onClick.AddListener(() => ShowAnimalSelection(idx));
            }
        }
    }

    public void ShowAnimalSelection(int locID)
    {
        DialogSystem.Instance.OpenAnimalSelection();
        foreach (Transform child in DialogSystem.Instance.animalListContainer) Destroy(child.gameObject);
        
        foreach (QuestInfo info in allQuests.FindAll(q => q.locationIndex == locID)) 
        {
            GameObject card = Instantiate(DialogSystem.Instance.animalRowPrefab, DialogSystem.Instance.animalListContainer);
            int mIdx = (info.locationIndex * 10) + info.missionIndex;
            bool canPlay = (totalCompletedMissions >= mIdx);
            bool alreadyDone = (totalCompletedMissions > mIdx);
            card.GetComponent<QuestCard>().Setup(info, this, canPlay && !alreadyDone);
        }
    }

    private void SetupOption1(string text, UnityEngine.Events.UnityAction action)
    {
        DialogSystem.Instance.option1BTN.GetComponentInChildren<TextMeshProUGUI>().text = text;
        DialogSystem.Instance.option1BTN.onClick.RemoveAllListeners();
        DialogSystem.Instance.option1BTN.onClick.AddListener(action);
    }

    public void EndConversation()
    {
        isTalkingWithPlayer = false;
        if(PlayerMovement.Instance != null) PlayerMovement.Instance.canControl = true;

        if (CameraSystem.Instance != null) CameraSystem.Instance.EnableConversationMode(false);

        DialogSystem.Instance.CloseAllPanels();
    }
}