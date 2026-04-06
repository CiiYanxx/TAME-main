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
    public Button backToNPCBTN; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CloseAllPanels();

        // I-setup ang listener para sa Back Button
        if (backToNPCBTN != null)
        {
            backToNPCBTN.onClick.RemoveAllListeners(); // Siguraduhin na walang duplicate listeners
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

    // --- OPTIMIZATION: Iwas UI Flicker at Lag ---
    public void CloseAllPanels()
    {
        if (dialogPanel && dialogPanel.activeSelf) dialogPanel.SetActive(false);
        if (locationSelectionPanel && locationSelectionPanel.activeSelf) locationSelectionPanel.SetActive(false);
        if (animalSelectionPanel && animalSelectionPanel.activeSelf) animalSelectionPanel.SetActive(false);
    }

    public void OpenDialogUI()
    {
        if (dialogPanel != null && dialogPanel.activeSelf) return; // Wag nang i-open kung open na
        CloseAllPanels();
        if (dialogPanel) dialogPanel.SetActive(true);
    }

    public void OpenLocationSelection()
    {
        if (locationSelectionPanel != null && locationSelectionPanel.activeSelf) return;
        CloseAllPanels();
        if (locationSelectionPanel) locationSelectionPanel.SetActive(true);
    }

    public void OpenAnimalSelection()
    {
        if (animalSelectionPanel != null && animalSelectionPanel.activeSelf) return;
        CloseAllPanels();
        if (animalSelectionPanel) animalSelectionPanel.SetActive(true);
    }

    // DAGDAG: Gamitin ito sa NPC script para linisin ang listahan nang mas mabilis
    public void ClearAnimalList()
    {
        if (animalListContainer == null) return;
        
        // Mas mabilis ito kaysa sa normal na Destroy sa loob ng foreach
        for (int i = animalListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(animalListContainer.GetChild(i).gameObject);
        }
    }
}