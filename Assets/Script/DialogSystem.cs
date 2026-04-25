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

    // 🔥 NEW: CLEAN SPAWN SYSTEM
    private List<GameObject> spawnedRows = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CloseAllPanels();

        // LOCATION BUTTONS
        for (int i = 0; i < locationCards.Length; i++)
        {
            int index = i;

            if (locationCards[i] != null)
            {
                locationCards[i].onClick.RemoveAllListeners();
                locationCards[i].onClick.AddListener(() =>
                {
                    Debug.Log("Location Card " + index + " clicked!");
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
    // 1st STEP: MAIN DIALOG
    // ==========================================
    public void OpenDialogUI()
    {
        CloseAllPanels();
        dialogPanel.SetActive(true);

        if (TutorialController.Instance != null && NPC.Instance.totalCompletedMissions == 0)
        {
            TutorialController.Instance.ShowArrowOnIndex(0);
        }
    }

    // ==========================================
    // 2nd STEP: MISSION PREVIEW
    // ==========================================
    public void OpenMissionPreview(QuestInfo info, NPC npc)
    {
        CloseAllPanels();
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
        {
            descriptionText.text = info.missionDescription;
        }

        confirmPreviewBtn.onClick.RemoveAllListeners();
        confirmPreviewBtn.onClick.AddListener(() =>
        {
            npc.AcceptMission(info);
        });

        if (TutorialController.Instance != null && NPC.Instance.totalCompletedMissions == 0)
        {
            TutorialController.Instance.ShowArrowOnIndex(3);
        }
    }

    // ==========================================
    // 3rd STEP: LOCATION
    // ==========================================
    public void OpenLocationSelection()
    {
        CloseAllPanels();
        locationSelectionPanel.SetActive(true);
        locationPanel.SetActive(true);

        if (TutorialController.Instance != null && NPC.Instance.totalCompletedMissions == 0)
        {
            TutorialController.Instance.ShowArrowOnIndex(1);
        }
    }

    // ==========================================
    // 4th STEP: ANIMAL SELECTION (FIXED)
    // ==========================================
    public void OpenAnimalSelection(int locationIndex)
    {
        CloseAllPanels();
        animalSelectionPanel.SetActive(true);

        SpawnAnimalList(locationIndex);

        if (TutorialController.Instance != null && NPC.Instance.totalCompletedMissions == 0)
        {
            TutorialController.Instance.ShowArrowOnIndex(2);
        }
    }

    // 🔥 SPAWN SYSTEM (FIXED)
    void SpawnAnimalList(int locationIndex)
    {
        ClearAnimalList();

        if (NPC.Instance == null) return;

        foreach (QuestInfo info in NPC.Instance.allQuests)
        {
            if (info.locationIndex != locationIndex) continue;

            GameObject row = Instantiate(animalRowPrefab, animalListContainer);
            spawnedRows.Add(row);

            // 🔥 VERY IMPORTANT PART
            QuestCard card = row.GetComponent<QuestCard>();

            if (card != null)
            {
                card.Setup(info, NPC.Instance, true);
            }
            else
            {
                Debug.LogError("Walang QuestCard script sa prefab mo!");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(animalListContainer.GetComponent<RectTransform>());
    }

    // 🔥 CLEAR SYSTEM (FIXED)
    public void ClearAnimalList()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null) Destroy(row);
        }

        spawnedRows.Clear();

        // EXTRA CLEAN (safety)
        for (int i = animalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(animalListContainer.GetChild(i).gameObject);
        }
    }

    // ==========================================
    // CLOSE ALL
    // ==========================================
    public void CloseAllPanels()
    {
        dialogPanel.SetActive(false);
        locationSelectionPanel.SetActive(false);
        animalSelectionPanel.SetActive(false);
        previewPanel.SetActive(false);
        locationPanel.SetActive(false);

        if (TutorialController.Instance != null)
            TutorialController.Instance.HideArrowUI();
    }
}