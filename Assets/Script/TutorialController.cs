using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public RectTransform arrow;

    [Header("Buttons")]
    public Button continueButton;
    public Button skipButton;

    private int stepIndex = 0;
    private bool isTutorialActive = false;

    private Transform targetUI;

    private string prefsKey = "Tutorial_Skipped";

    void Awake()
    {
        Instance = this;

        tutorialPanel.SetActive(false);

        continueButton.onClick.AddListener(NextStep);
        skipButton.onClick.AddListener(SkipTutorial);
    }

    public void StartTutorial()
    {
        if (PlayerPrefs.GetInt(prefsKey, 0) == 1)
            return; // skipped permanently

        isTutorialActive = true;
        stepIndex = 0;

        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        ShowStep();
    }

    void ShowStep()
    {
        switch (stepIndex)
        {
            case 0:
                tutorialText.text = "Hold Sneak button to approach animals slowly.";
                targetUI = FindUI("SneakButton");
                break;

            case 1:
                tutorialText.text = "Press Feed button when trust is full.";
                targetUI = FindUI("FeedButton");
                break;

            case 2:
                tutorialText.text = "Complete the mini-game to rescue the animal!";
                targetUI = FindUI("TameButton");
                break;

            default:
                EndTutorial();
                return;
        }

        MoveArrow();
    }

    void MoveArrow()
    {
        if (targetUI == null || arrow == null) return;

        Vector3 screenPos = targetUI.position;
        arrow.position = screenPos + new Vector3(0, 80f, 0);
    }

    void NextStep()
    {
        stepIndex++;
        ShowStep();
    }

    void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void SkipTutorial()
    {
        PlayerPrefs.SetInt(prefsKey, 1);
        PlayerPrefs.Save();

        EndTutorial();
    }

    Transform FindUI(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj.transform;
        return null;
    }
}