using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [Header("1st Step: Main Dialog")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public Button option1BTN; 
    public Button option2BTN; 

    [Header("2nd Step: Mission Preview Panel")]
    public GameObject previewPanel;
    public Image previewAnimalImage;
    public TextMeshProUGUI detailsText;   
    public TextMeshProUGUI descriptionText; 
    public Button confirmPreviewBtn;

    [Header("3rd Step: Location Selection")]
    public GameObject locationSelectionPanel;
    public Button[] locationCards; 
    public GameObject[] locationLocks; 

    [Header("4th Step: Animal Selection")]
    public GameObject animalSelectionPanel;
    public GameObject animalRowPrefab; 
    public Transform animalListContainer; 

    [Header("Exit Logic")]
    public Button backToNPCBTN; 

    [Header("Panels")]
    public GameObject locationPanel;

    [Header("Progress Lock")]
    public int requiredLevelForNextLocation = 10;

    private List<GameObject> spawnedRows = new List<GameObject>();

    // 🔥 FIX: prevent duplicate arrow spam
    private int lastArrowIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CloseAllPanels();
        UpdateLocationLocks();

        for (int i = 0; i < locationCards.Length; i++)
        {
            int index = i;

            if (locationCards[i] != null)
            {
                locationCards[i].onClick.RemoveAllListeners();
                locationCards[i].onClick.AddListener(() =>
                {
                    Debug.Log("Location Card " + index + " clicked!");

                    int playerLevel = PlayerPrefs.GetInt("Player_Level", 1);
                    bool locked = playerLevel < (requiredLevelForNextLocation * (index + 1));

                    if (locked)
                    {
                        Debug.LogWarning($"Location {index} is LOCKED");
                        return;
                    }

                    OpenAnimalSelection(index);
                });
            }
        }

        if (backToNPCBTN != null)
        {
            backToNPCBTN.onClick.AddListener(() =>
            {
                if (NPC.Instance != null) NPC.Instance.EndConversation();
                else CloseAllPanels();
            });
        }
    }
    // ==========================================
    void HideTutorialArrow()
    {
        if (TutorialController.Instance != null)
        {
            TutorialController.Instance.HideArrowUI();
        }

        lastArrowIndex = -1; // 🔥 RESET FIX
    }

    // ==========================================
    public void OpenDialogUI()
    {
        CloseAllPanels();
        HideTutorialArrow();

        dialogPanel.SetActive(true);

        TriggerArrow(0);
    }

    // ==========================================
    public void OpenMissionPreview(QuestInfo info, NPC npc)
    {
        CloseAllPanels();
        HideTutorialArrow();

        previewPanel.SetActive(true);

        if (previewAnimalImage && info.animalFullPreview != null)
            previewAnimalImage.sprite = info.animalFullPreview;

        if (detailsText)
        {
            detailsText.text = info.animalDetails
                .Replace("BREED:", "<color=#40A6CE>BREED:</color>")
                .Replace("AGE:", "<color=#40A6CE>AGE:</color>")
                .Replace("STATUS:", "<color=#40A6CE>STATUS:</color>")
                .Replace("COLOR:", "<color=#40A6CE>COLOR:</color>");
        }

        if (descriptionText)
            descriptionText.text = info.missionDescription;

        confirmPreviewBtn.onClick.RemoveAllListeners();
        confirmPreviewBtn.onClick.AddListener(() =>
        {
            npc.AcceptMission(info);
        });

        TriggerArrow(3);
    }

    // ==========================================
    public void OpenLocationSelection()
    {
        CloseAllPanels();
        HideTutorialArrow();

        locationSelectionPanel.SetActive(true);
        locationPanel.SetActive(true);

        TriggerArrow(1);
    }

    // ==========================================
    public void OpenAnimalSelection(int locationIndex)
    {
        CloseAllPanels();

        // 🔥 ADD THIS (kills arrow before UI changes)
        if (TutorialController.Instance != null)
            TutorialController.Instance.HideArrowUI();

        animalSelectionPanel.SetActive(true);

        SpawnAnimalList(locationIndex);

        TriggerArrow(2);
    }

    void SpawnAnimalList(int locationIndex)
    {
        ClearAnimalList();

        if (NPC.Instance == null)
        {
            Debug.LogError("[DialogSystem] NPC is NULL");
            return;
        }

        Debug.Log($"[DialogSystem] Loading animals for locationIndex = {locationIndex}");

        bool foundAny = false;

        foreach (QuestInfo info in NPC.Instance.allQuests)
        {
            Debug.Log($"Checking Quest: {info.targetAnimalName} | loc={info.locationIndex}");

            if (info.locationIndex != locationIndex) continue;

            foundAny = true;

            GameObject row = Instantiate(animalRowPrefab, animalListContainer);
            spawnedRows.Add(row);

            QuestCard card = row.GetComponent<QuestCard>();

            if (card != null)
                card.Setup(info, NPC.Instance, true);
        }

        if (!foundAny)
        {
            Debug.LogWarning($"[DialogSystem] NO QUEST FOUND for location {locationIndex}");
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            animalListContainer.GetComponent<RectTransform>()
        );
    }

    public void ClearAnimalList()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null) Destroy(row);
        }

        spawnedRows.Clear();

        for (int i = animalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(animalListContainer.GetChild(i).gameObject);
        }
    }

    // ==========================================
    public void CloseAllPanels()
    {
        dialogPanel.SetActive(false);
        locationSelectionPanel.SetActive(false);
        animalSelectionPanel.SetActive(false);
        previewPanel.SetActive(false);
        locationPanel.SetActive(false);

        HideTutorialArrow();
    }

    void TriggerArrow(int index)
    {
        if (TutorialController.Instance == null) return;
        if (NPC.Instance != null && NPC.Instance.totalCompletedMissions > 0) return;

        if (lastArrowIndex == index) return; // 🔥 PREVENT DUPLICATE

        lastArrowIndex = index;

        TutorialController.Instance.ShowArrowOnIndex(index);
    }

    void UpdateLocationLocks()
    {
        int playerLevel = PlayerPrefs.GetInt("Player_Level", 1);

        for (int i = 0; i < locationCards.Length; i++)
        {
            bool locked = playerLevel < (requiredLevelForNextLocation * (i + 1));

            if (locationCards[i] != null)
                locationCards[i].interactable = !locked;

            if (locationLocks != null && i < locationLocks.Length && locationLocks[i] != null)
                locationLocks[i].SetActive(locked);
        }
    }

}