using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointerController : MonoBehaviour
{
    public static PointerController Instance { get; private set; }

    [Header("UI References")]
    public RectTransform safeZoneRect;
    public RectTransform travelAreaRect;
    public GameObject minigameUIContainer;
    
    [Header("Counter Displays (Numbers Only)")]
    public TextMeshProUGUI successText; 
    public TextMeshProUGUI failText;    

    [Header("Outcome Panel References")]
    public GameObject outcomePanel; 
    public TextMeshProUGUI outcomeText;
    public Button outcomeContinueBtn; 

    [Header("Player Control UI")]
    public GameObject playerJoystick; 
    public GameObject playerInteractButton; 
    public Button tameButton; 

    // MGA VARIABLES NA GALING NA SA QUEST INFO (Hindi na hardcoded dito)
    private float moveSpeed; 
    private int attemptsRequired;
    private int maxFailedAttempts; 

    private float travelRange;
    private bool isActive = false;
    private int successfulAttempts = 0, failedAttemptsCount = 0;
    private bool lastResultWasSuccess = false;
    private RectTransform pointerTransform;
    private AnimalInteractable animalInteractable;

    private enum TameState
    {
        None,
        Ready,
        Playing
    }

    private TameState currentState = TameState.None;
    

    void Awake()
    {
        if (Instance == null) Instance = this;
        pointerTransform = GetComponent<RectTransform>();
        if (minigameUIContainer != null) minigameUIContainer.SetActive(false);
        if (outcomePanel != null) outcomePanel.SetActive(false); 
        
        if (tameButton != null) {
            tameButton.gameObject.SetActive(false);
            tameButton.onClick.RemoveAllListeners();
            tameButton.onClick.AddListener(OnTameButtonClicked);
        }
    }

    public void ShowTamePrompt(AnimalInteractable caller)
    {
        animalInteractable = caller;
        ToggleMainControls(false); 

        if (tameButton != null)
        {
            tameButton.gameObject.SetActive(true);

            tameButton.onClick.RemoveAllListeners();
            tameButton.onClick.AddListener(OnTameButtonClicked);
        }

        currentState = TameState.Ready;

        if (TutorialController.Instance != null)
            TutorialController.Instance.Tutorial7_Tame();
    }

    private void OnTameButtonClicked()
    {
        if (currentState == TameState.Ready)
        {
            // ✅ TRIGGER TUTORIAL 8
            if (TutorialController.Instance != null)
                TutorialController.Instance.Tutorial8_Minigame();

            StartMinigame();
        }
        else if (currentState == TameState.Playing)
        {
            AttemptRescue();
        }
    }

    public void ToggleMainControls(bool state)
    {
        if (playerJoystick != null) playerJoystick.SetActive(state);
        if (playerInteractButton != null) playerInteractButton.SetActive(state);
    }

    void Update()
    {
        if (!isActive) return;

        float halfRange = travelRange / 2f;
        float xPos = Mathf.PingPong(Time.time * moveSpeed, travelRange) - halfRange;
        pointerTransform.localPosition = new Vector3(xPos, pointerTransform.localPosition.y, 0f); 
    }

    public void StartMinigame()
    {
        if (travelAreaRect != null) travelRange = travelAreaRect.rect.width;

        if (animalInteractable != null && animalInteractable.currentQuest != null)
        {
            QuestInfo q = animalInteractable.currentQuest;
            attemptsRequired = q.requiredSuccesses;
            maxFailedAttempts = q.maxFailures;
            moveSpeed = q.pointerSpeed;
        }
        else
        {
            attemptsRequired = 5;
            maxFailedAttempts = 3;
            moveSpeed = 300f;
        }

        successfulAttempts = 0; 
        failedAttemptsCount = 0;
        isActive = true;

        minigameUIContainer.SetActive(true);

        // ❗ IMPORTANT: wag mo na itago button
        tameButton.gameObject.SetActive(true);

        // 🔥 Change role
        tameButton.onClick.RemoveAllListeners();
        tameButton.onClick.AddListener(OnTameButtonClicked);

        currentState = TameState.Playing;

        pointerTransform.localPosition = new Vector3(0f, pointerTransform.localPosition.y, 0f); 
        
        UpdateCounterUI(); 
        RandomizeSafeZonePosition();
    }
    public void AttemptRescue()
    {
        if (!isActive) return;

        float pointerX = pointerTransform.localPosition.x;
        float safeZoneX = safeZoneRect.localPosition.x;
        float safeZoneHalfWidth = safeZoneRect.rect.width / 2f;

        bool success = (pointerX >= (safeZoneX - safeZoneHalfWidth)) && 
                       (pointerX <= (safeZoneX + safeZoneHalfWidth));

        if (success) {
            successfulAttempts++;
            UpdateCounterUI();
            // Tinitingnan ang target value mula sa QuestInfo
            if (successfulAttempts >= attemptsRequired) EndMinigame(true);
            else ResetPointerAndAdvance();
        } else {
            failedAttemptsCount++;
            UpdateCounterUI();
            // Tinitingnan ang limit mula sa QuestInfo
            if (failedAttemptsCount >= maxFailedAttempts) EndMinigame(false);
            else ResetPointerAndAdvance();
        }
    }

    private void UpdateCounterUI()
    {
        if (successText != null) 
        {
            // Ipinapakita kung ilan na lang ang kailangan base sa QuestInfo data
            int remaining = Mathf.Max(0, attemptsRequired - successfulAttempts);
            successText.text = remaining.ToString();
        }

        if (failText != null) 
        {
            int remainingLives = Mathf.Max(0, maxFailedAttempts - failedAttemptsCount);
            failText.text = remainingLives.ToString();
        }
    }

    private void EndMinigame(bool missionSuccess)
    {
        if (!isActive) return;

        isActive = false;
        lastResultWasSuccess = missionSuccess;

        minigameUIContainer.SetActive(false);

        // ❗ hide button after game
        if (tameButton != null)
            tameButton.gameObject.SetActive(false);

        currentState = TameState.None;

        if (animalInteractable != null)
            animalInteractable.ReportMissionOutcome(missionSuccess);

        ShowOutcomeFirstDialog();
    }

    private void ShowOutcomeFirstDialog() 
    { 
        if (outcomePanel == null) return;
        
        if (lastResultWasSuccess) 
            outcomeText.text = "Congratulations!\nYou successfully rescue the stray animal!";
        else 
            outcomeText.text = "Oh no!\nYou scared away the stray animal!";

        outcomePanel.SetActive(true); 
        outcomeContinueBtn.onClick.RemoveAllListeners();
        outcomeContinueBtn.onClick.AddListener(ShowOutcomeSecondDialog);
    }

    private void ShowOutcomeSecondDialog() 
    { 
        if (lastResultWasSuccess) outcomeText.text = "Go back to Dr. Kevin so he can assess the rescued animal and check their condition";
        else outcomeText.text = "Go back to Dr. Kevin to restart the rescue mission";

        outcomeContinueBtn.onClick.RemoveAllListeners();
        outcomeContinueBtn.onClick.AddListener(CloseOutcomePanel);
    }

    private void CloseOutcomePanel() 
    { 
        outcomePanel.SetActive(false); 
        ToggleMainControls(true); 
        if (NPC.Instance != null) NPC.Instance.SetMissionReturned(true, lastResultWasSuccess); 
    }

    private void RandomizeSafeZonePosition() { 
        float maxOffset = (travelRange / 2f) - (safeZoneRect.rect.width / 2f); 
        float newX = Random.Range(-maxOffset, maxOffset); 
        safeZoneRect.localPosition = new Vector3(newX, safeZoneRect.localPosition.y, 0f); 
    }

    private void ResetPointerAndAdvance() { 
        pointerTransform.localPosition = new Vector3(0f, pointerTransform.localPosition.y, 0f); 
        RandomizeSafeZonePosition(); 
    }
}