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

    public void OpenMissionPreview(QuestInfo info, NPC npc)
    {
        CloseAllPanels();
        if (previewPanel) previewPanel.SetActive(true);

        if (previewAnimalImage && info.animalFullPreview != null) 
            previewAnimalImage.sprite = info.animalFullPreview;
        
        // AUTO-COLORING LOGIC
        if (detailsText)
        {
            // Details Labels: Light Blue (#40A6CE)
            string coloredDetails = info.animalDetails
                .Replace("BREED:", "<color=#40A6CE>BREED:</color>")
                .Replace("AGE:", "<color=#40A6CE>AGE:</color>")
                .Replace("STATUS:", "<color=#40A6CE>STATUS:</color>")
                .Replace("COLOR:", "<color=#40A6CE>COLOR:</color>")
                .Replace("CONDITION:", "<color=#40A6CE>CONDITION:</color>")
                .Replace("BEHAVIOR:", "<color=#40A6CE>BEHAVIOR:</color>")
                .Replace("DIET:", "<color=#40A6CE>DIET:</color>");
            detailsText.text = coloredDetails;
        }

        if (descriptionText)
        {
            // Description Label: Light Blue (#40A6CE)
            // Hint Label & Content: Green (#00FF00)
            string rawDesc = info.missionDescription;

            string coloredDesc = rawDesc
                .Replace("DESCRIPTION:", "<color=#40A6CE>DESCRIPTION:</color>");

            // Para sa Hint, pati yung kasunod na text ay magiging green hanggang sa dulo ng line
            if (coloredDesc.Contains("HINT:"))
            {
                coloredDesc = coloredDesc.Replace("HINT:", "<color=#00FF00>HINT:");
                coloredDesc += "</color>"; // Isinasara ang green tag para sa hint part
            }

            descriptionText.text = coloredDesc;
        }

        if (confirmPreviewBtn != null)
        {
            confirmPreviewBtn.onClick.RemoveAllListeners();
            confirmPreviewBtn.onClick.AddListener(() => {
                if (previewPanel) previewPanel.SetActive(false);
                OpenDialogUI(); 
                npc.AcceptMission(info); 
            });
        }
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