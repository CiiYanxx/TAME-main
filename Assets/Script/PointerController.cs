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

    [Header("Default Settings (Fallback)")]
    public float moveSpeed = 300f; 
    public int attemptsRequired = 5;
    public int maxFailedAttempts = 3; 

    private float travelRange;
    private bool isActive = false;
    private int successfulAttempts = 0, failedAttemptsCount = 0;
    private bool lastResultWasSuccess = false;
    private RectTransform pointerTransform;
    private AnimalInteractable animalInteractable;

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
        if (tameButton != null) tameButton.gameObject.SetActive(true);
    }

    private void OnTameButtonClicked()
    {
        if (tameButton != null) tameButton.gameObject.SetActive(false);
        StartMinigame();
    }

    public void ToggleMainControls(bool state)
    {
        if (playerJoystick != null) playerJoystick.SetActive(state);
        if (playerInteractButton != null) playerInteractButton.SetActive(state);
    }

    void Update()
    {
        if (!isActive) return;

        // Accurate PingPong movement based on the parent area's width
        float halfRange = travelRange / 2f;
        float xPos = Mathf.PingPong(Time.time * moveSpeed, travelRange) - halfRange;
        pointerTransform.localPosition = new Vector3(xPos, pointerTransform.localPosition.y, 0f); 
    }

    public void StartMinigame()
    {
        if (travelAreaRect != null) travelRange = travelAreaRect.rect.width;
        
        if (animalInteractable != null && animalInteractable.currentQuest != null)
        {
            attemptsRequired = animalInteractable.currentQuest.requiredSuccesses;
            maxFailedAttempts = animalInteractable.currentQuest.maxFailures;
            moveSpeed = animalInteractable.currentQuest.pointerSpeed;
        }

        successfulAttempts = 0; 
        failedAttemptsCount = 0;
        isActive = true;
        minigameUIContainer.SetActive(true);
        pointerTransform.localPosition = new Vector3(0f, pointerTransform.localPosition.y, 0f); 
        
        UpdateCounterUI();
        RandomizeSafeZonePosition();
    }
    
    public void AttemptRescue()
    {
        if (!isActive) return;

        // ACCURACY FIX: Gamitin ang LocalPosition para parehas sila ng reference point
        float pointerX = pointerTransform.localPosition.x;
        float safeZoneX = safeZoneRect.localPosition.x;
        
        // Gamitin ang rect.width para makuha ang actual size kahit anong scaling
        float safeZoneHalfWidth = safeZoneRect.rect.width / 2f;

        // Detection logic
        bool success = (pointerX >= (safeZoneX - safeZoneHalfWidth)) && 
                       (pointerX <= (safeZoneX + safeZoneHalfWidth));

        if (success) {
            successfulAttempts++;
            UpdateCounterUI();
            if (successfulAttempts >= attemptsRequired) EndMinigame(true);
            else ResetPointerAndAdvance();
        } else {
            failedAttemptsCount++;
            UpdateCounterUI();
            if (failedAttemptsCount >= maxFailedAttempts) EndMinigame(false);
            else ResetPointerAndAdvance();
        }
    }

    private void UpdateCounterUI()
    {
        if (successText != null) 
        {
            int remainingSuccess = attemptsRequired - successfulAttempts;
            successText.text = remainingSuccess.ToString();
        }

        if (failText != null) 
        {
            int remainingLives = maxFailedAttempts - failedAttemptsCount;
            failText.text = remainingLives.ToString();
        }
    }

    private void EndMinigame(bool missionSuccess)
    {
        if (!isActive) return;
        isActive = false;
        lastResultWasSuccess = missionSuccess;
        minigameUIContainer.SetActive(false);
        
        if (animalInteractable != null) animalInteractable.ReportMissionOutcome(missionSuccess);
        
        ShowOutcomeFirstDialog();
    }

    private void ShowOutcomeFirstDialog() 
    { 
        if (outcomePanel == null) return;
        
        if (lastResultWasSuccess) 
            outcomeText.text = "Congratulations!\nYou successfully Rescue the Stray animals!";
        else 
            outcomeText.text = "Oh no!\nYou failed to rescue the stray animals!";

        outcomePanel.SetActive(true); 
        outcomeContinueBtn.onClick.RemoveAllListeners();
        outcomeContinueBtn.onClick.AddListener(ShowOutcomeSecondDialog);
    }

    private void ShowOutcomeSecondDialog() 
    { 
        if (lastResultWasSuccess) outcomeText.text = "Go back to Dr. Kevin for new Rescue Mission";
        else outcomeText.text = "Go back to Dr. Kevin to restart the Rescue Mission";

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
        // Siguraduhin na ang safe zone ay hindi lalabas sa travel area
        float maxOffset = (travelRange / 2f) - (safeZoneRect.rect.width / 2f); 
        float newX = Random.Range(-maxOffset, maxOffset); 
        safeZoneRect.localPosition = new Vector3(newX, safeZoneRect.localPosition.y, 0f); 
    }

    private void ResetPointerAndAdvance() { 
        pointerTransform.localPosition = new Vector3(0f, pointerTransform.localPosition.y, 0f); 
        RandomizeSafeZonePosition(); 
    }
}