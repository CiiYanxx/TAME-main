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

    [Header("2nd Step: Location Selection")]
    public GameObject locationSelectionPanel;
    public Button[] locationCards; 
    // BAGONG VARIABLE: I-drag dito ang Lock Images ng bawat Location Card
    public GameObject[] locationLocks; 

    [Header("3rd Step: Animal Selection")]
    public GameObject animalSelectionPanel;
    public GameObject animalRowPrefab; 
    public Transform animalListContainer; 

    [Header("Exit Logic")]
    // Ang button na magsasara ng lahat pabalik sa NPC gameplay interaction
    public Button backToNPCBTN; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CloseAllPanels();

        // I-setup ang listener para sa Back Button
        if (backToNPCBTN != null)
        {
            backToNPCBTN.onClick.AddListener(() => {
                if (NPC.Instance != null) 
                {
                    NPC.Instance.EndConversation(); 
                }
                else 
                {
                    CloseAllPanels();
                }
            });
        }
    }

    public void CloseAllPanels()
    {
        if (dialogPanel) dialogPanel.SetActive(false);
        if (locationSelectionPanel) locationSelectionPanel.SetActive(false);
        if (animalSelectionPanel) animalSelectionPanel.SetActive(false);
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
}