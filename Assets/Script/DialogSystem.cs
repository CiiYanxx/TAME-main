using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; private set; }

    [Header("1st Step: Main Dialog")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public Button option1BTN; 
    public Button option2BTN; 

    [Header("Typing Effect")]
    public float typingSpeed = 0.02f;
    private string fullText;
    private Coroutine typingCoroutine;

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

    [Header("Mission Unlock Requirements")]
    public int[] requiredMissionsPerLocation = new int[3];

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

                    int completed = 0;

                    if (NPC.Instance != null)
                        completed = NPC.Instance.totalCompletedMissions;

                    bool locked = false;

                    if (index < requiredMissionsPerLocation.Length)
                        locked = completed < requiredMissionsPerLocation[index];

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
                if (NPC.Instance != null)
                    NPC.Instance.ShowExitDialogue();
                else
                    CloseAllPanels();
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

        // =========================
        // DETAILS TEXT COLORS
        // =========================
        if (detailsText)
        {
            detailsText.text = info.animalDetails
                .Replace("DESCRIPTION:", "<b><color=#0074A2>DESCRIPTION:</color></b>")
                .Replace("BREED:", "<b><color=#0074A2>BREED:</color></b>")
                .Replace("AGE:", "<b><color=#0074A2>AGE:</color></b>")
                .Replace("COLOR:", "<b><color=#0074A2>COLOR:</color></b>")
                .Replace("CONDITION:", "<b><color=#0074A2>CONDITION:</color></b>")
                .Replace("BEHAVIOR:", "<b><color=#0074A2>BEHAVIOR:</color></b>")
                .Replace("DIET:", "<b><color=#0074A2>DIET:</color></b>");
        }

        // =========================
        // DESCRIPTION TEXT COLORS
        // =========================
        if (descriptionText)
        {
            string desc = info.missionDescription;

            // kulay green lahat
            desc = $"<color=#000000>{desc}</color>";

            // pero ibang kulay ang HINT:
            desc = desc.Replace(
                "HINT:",
                "</color><color=#000000>HINT:</color><color=#000000>"
            );

            // close final color tag
            desc += "</color>";

            descriptionText.text = desc;
        }

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

        UpdateLocationLocks(); // ADD THIS

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
            Debug.LogError("NPC NULL");
            return;
        }

        if (animalRowPrefab == null)
        {
            Debug.LogError("animalRowPrefab NULL");
            return;
        }

        if (animalListContainer == null)
        {
            Debug.LogError("animalListContainer NULL");
            return;
        }

        foreach (QuestInfo info in NPC.Instance.allQuests)
        {
            if (info.locationIndex != locationIndex)
                continue;

            Debug.Log("Creating Row For: " + info.targetAnimalName);

            GameObject row = Instantiate(animalRowPrefab, animalListContainer);

            QuestCard card = row.GetComponent<QuestCard>();

            if (card == null)
            {
                Debug.LogError("QuestCard missing on prefab");
                continue;
            }

            try
            {
                card.Setup(info, NPC.Instance, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError("QuestCard Setup Error on " + info.targetAnimalName);
                Debug.LogError(e);
            }

            spawnedRows.Add(row);
        }
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

        int completed = 0;

         if (NPC.Instance != null)
            completed = NPC.Instance.totalCompletedMissions;

        for (int i = 0; i < locationCards.Length; i++)
        {
            bool locked = false;

            if (i < requiredMissionsPerLocation.Length)
                    locked = completed < requiredMissionsPerLocation[i];

            if (locationCards[i] != null)
                    locationCards[i].interactable = !locked;

            if (locationLocks != null &&
                 i < locationLocks.Length &&
                 locationLocks[i] != null)
             {
                locationLocks[i].SetActive(locked);
            }
        }
    }
    

    public void RefreshLocks()
    {
        UpdateLocationLocks();
    }

    public void SetDialogText(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        dialogText.text = "";

        bool isInsideTag = false;

        foreach (char c in text)
        {
            if (c == '<')
                isInsideTag = true;

            dialogText.text += c;

            if (!isInsideTag)
                yield return new WaitForSeconds(typingSpeed);

            if (c == '>')
                isInsideTag = false;
        }
    }
}