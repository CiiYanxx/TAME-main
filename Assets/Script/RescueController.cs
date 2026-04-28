using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class RescueController : MonoBehaviour
{
    public static RescueController Instance { get; private set; }

    [Header("Noise Meter UI")]
    public GameObject noiseMeterGroup;
    public Image noiseMeterFill;
    public GameObject sneakButton;
    public Button feedButton;

    [Header("Fail Outcome Panel")]
    public GameObject failPanel;
    public TMPro.TextMeshProUGUI failText;
    public Button failContinueBtn;

    [Header("Hint UI")]
    public GameObject hintPanel;
    public TMPro.TextMeshProUGUI hintText;

    [Header("Settings & Prefabs")]
    public List<GameObject> animalPrefabs = new List<GameObject>();

    [Header("Player Controls")]
    public GameObject moveJoystick;
    public GameObject lookJoystick;

    public float foodForwardOffset = 1.5f;
    public float foodVerticalOffset = 0.1f;

    private GameObject currentAnimal = null;
    private NPC activeNPC = null;
    private QuestInfo currentInfo = null;
    private AnimalMissionLogic activeMissionLogic;

    private bool lastMissionSuccess = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (failPanel != null)
            failPanel.SetActive(false);

        CleanupMission();
    }

    public void StartMission(NPC npc, QuestInfo info)
    {
        CleanupMission();

        activeNPC = npc;
        currentInfo = info;

        // ✅ ENABLE PLAYER CONTROLS (NEW MISSION RESET)
        if (moveJoystick != null)
            moveJoystick.SetActive(true);

        if (lookJoystick != null)
            lookJoystick.SetActive(true);

        if (sneakButton != null)
            sneakButton.SetActive(true);

        GameObject prefab = animalPrefabs.Find(
            p => p.name.ToLower().Trim() == info.targetAnimalName.ToLower().Trim()
        );

        if (prefab != null)
        {
            currentAnimal = Instantiate(
                prefab,
                info.spawnPosition,
                Quaternion.Euler(info.animalRotation)
            );

            activeMissionLogic = currentAnimal.GetComponent<AnimalMissionLogic>();

            if (activeMissionLogic == null)
                activeMissionLogic = currentAnimal.AddComponent<AnimalMissionLogic>();

            activeMissionLogic.SetupMission(info);

            AnimalInteractable interactable =
                currentAnimal.GetComponent<AnimalInteractable>();

            if (interactable != null)
                interactable.SetupQuest(info);
        }
    }

    public void UpdateNoiseMeter(bool isVisible, Color stateColor, float fillValue)
    {
        if (noiseMeterGroup == null || noiseMeterFill == null)
            return;

        if (noiseMeterGroup.activeSelf != isVisible)
            noiseMeterGroup.SetActive(isVisible);

        if (!isVisible)
            return;

        noiseMeterFill.fillAmount = fillValue;
        noiseMeterFill.color = stateColor;
    }

    public void ShowHint(string hint)
    {
        if (hintPanel == null || hintText == null) return;

        hintPanel.SetActive(true);
        hintText.text = hint;
    }

    public void HideHint()
    {
        if (hintPanel == null) return;

        if (!hintPanel.activeSelf) return;

        hintPanel.SetActive(false);
    }

    public void ReportMissionOutcome(bool success)
    {
        lastMissionSuccess = success;

        if (success && currentInfo != null)
        {
            var points = FindFirstObjectByType<RescuePointsHandler>();

            if (points != null)
                points.AddPoints(currentInfo.progressPointsReward);

            StartCoroutine(TriggerTutorial9Delay());
        }

        if (activeNPC != null)
            activeNPC.ReportQuestOutcome(success);

        if (noiseMeterGroup != null)
            noiseMeterGroup.SetActive(false);

        if (sneakButton != null)
            sneakButton.SetActive(false);

        if (feedButton != null)
            feedButton.gameObject.SetActive(false);

        // ❗ RESTORE CONTROL HERE (NOT EARLY)
        RestorePlayerControls();

        if (!success)
            ShowFailDialog();

        StartCoroutine(CleanupAfterDelay());
    }

    void ShowFailDialog()
    {
        if (failPanel == null)
            return;

        failPanel.SetActive(true);

        if (failText != null)
            failText.text =
                "Oh no!\nYou scared awaay the stray animal!";

        if (failContinueBtn != null)
        {
            failContinueBtn.onClick.RemoveAllListeners();
            failContinueBtn.onClick.AddListener(ShowFailSecondDialog);
        }
    }

    void ShowFailSecondDialog()
    {
        if (failText != null)
            failText.text =
                "Go back to Dr. Kevin to restart the Rescue Mission";

        if (failContinueBtn != null)
        {
            failContinueBtn.onClick.RemoveAllListeners();
            failContinueBtn.onClick.AddListener(CloseFailDialog);
        }
    }

    void CloseFailDialog()
    {
        if (failPanel != null)
            failPanel.SetActive(false);

        if (NPC.Instance != null)
            NPC.Instance.SetMissionReturned(true, false);
    }

    IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(10f);
        CleanupMission();
    }

    public void CleanupMission()
    {
        if (noiseMeterGroup != null)
            noiseMeterGroup.SetActive(false);

        if (sneakButton != null)
            sneakButton.SetActive(false);

        if (feedButton != null)
            feedButton.gameObject.SetActive(false);

        if (noiseMeterFill != null)
            noiseMeterFill.fillAmount = 0f;

        if (currentAnimal != null)
            Destroy(currentAnimal);

        currentAnimal = null;
        activeMissionLogic = null;

        PlayerPrefs.SetInt("IsMissionActive", 0);
        PlayerPrefs.DeleteKey("ActiveLocIdx");
        PlayerPrefs.DeleteKey("ActiveMissIdx");
        PlayerPrefs.Save();
    }

    public void AddGravity(GameObject obj)
    {
        Rigidbody rb =
            obj.GetComponent<Rigidbody>() ??
            obj.AddComponent<Rigidbody>();

        rb.useGravity = true;

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();
    }

    public void OnFullTrustReached()
    {
        if (moveJoystick != null)
            moveJoystick.SetActive(false);

        if (lookJoystick != null)
            lookJoystick.SetActive(false);

        // ✅ ADD THIS RESET PART
        if (moveJoystick != null)
        {
            var joy = moveJoystick.GetComponent<VirtualJoystick>();
            if (joy != null)
                joy.ResetJoystick();
        }

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canControl = false;
            PlayerMovement.Instance.HardStopMovement();
        }
    }

    private void RestorePlayerControls()
    {
        // reset joystick UI
        if (moveJoystick != null)
            moveJoystick.SetActive(true);

        if (lookJoystick != null)
            lookJoystick.SetActive(true);

        // reset player input state
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canControl = true;
        }
    }

            IEnumerator TriggerTutorial9Delay()
    {
        yield return new WaitForSeconds(5f);

        if (TutorialController.Instance != null)
            TutorialController.Instance.Tutorial9_Rescue();
    }
    
}