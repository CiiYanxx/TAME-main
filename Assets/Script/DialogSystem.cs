using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [Header("1st Step: Main Dialog")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public Button option1BTN; 
    public Button option2BTN; 

    [Header("NEW: Mission Preview Panel")]
    public GameObject previewPanel;
    public Image previewAnimalImage;
    public TextMeshProUGUI previewStatsText;
    public Button confirmPreviewBtn;

    [Header("2nd Step: Location Selection")]
    public GameObject locationSelectionPanel;
    public Button[] locationCards; 
    public GameObject[] locationLocks; 

    [Header("3rd Step: Animal Selection")]
    public GameObject animalSelectionPanel;
    public GameObject animalRowPrefab; 
    public Transform animalListContainer; 

    [Header("Exit Logic")]
    public Button backToNPCBTN; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CloseAllPanels();

        if (backToNPCBTN != null)
        {
            backToNPCBTN.onClick.AddListener(() => {
                if (NPC.Instance != null) NPC.Instance.EndConversation(); 
                else CloseAllPanels();
            });
        }
    }

    // --- STEP 2 LOGIC: Preview muna bago Dialog ---
    public void OpenMissionPreview(QuestInfo info, NPC npc)
    {
        CloseAllPanels();
        previewPanel.SetActive(true);

        if (previewAnimalImage) previewAnimalImage.sprite = info.animalIcon;
        if (previewStatsText) previewStatsText.text = $"Name: {info.targetAnimalName}\nProblem: {info.description}";

        confirmPreviewBtn.onClick.RemoveAllListeners();
        confirmPreviewBtn.onClick.AddListener(() => {
            previewPanel.SetActive(false);
            OpenDialogUI(); // Lilipat na sa Main Dialog
            npc.AcceptMission(info); // Dito lang talaga maa-accept
        });
    }

    public void CloseAllPanels()
    {
        if (dialogPanel) dialogPanel.SetActive(false);
        if (locationSelectionPanel) locationSelectionPanel.SetActive(false);
        if (animalSelectionPanel) animalSelectionPanel.SetActive(false);
        if (previewPanel) previewPanel.SetActive(false);
    }

    public void OpenDialogUI()
    {
        CloseAllPanels();
        if (dialogPanel) dialogPanel.SetActive(true);
    }

    public void OpenLocationSelection()
    {
        CloseAllPanels();
        if (locationSelectionPanel) locationSelectionPanel.SetActive(true);
    }

    public void OpenAnimalSelection()
    {
        CloseAllPanels();
        if (animalSelectionPanel) animalSelectionPanel.SetActive(true);
    }

    public void ClearAnimalList()
    {
        if (animalListContainer == null) return;
        for (int i = animalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(animalListContainer.GetChild(i).gameObject);
        }
    }
}