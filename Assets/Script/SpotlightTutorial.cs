using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpotlightTutorial : MonoBehaviour
{
    [System.Serializable]
    public struct TutorialStep
    {
        public string instruction;
        public RectTransform targetUI; // Ang button na gusto mong i-highlight
        public Vector2 holeSize;       // Laki ng butas
    }

    [Header("UI Elements")]
    public GameObject overlayPanel;
    public RectTransform spotlightHole;
    public TextMeshProUGUI instructionText;
    public CanvasGroup gameHUD;

    [Header("Steps")]
    public TutorialStep[] steps;
    private int currentStep = 0;

    void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        overlayPanel.SetActive(true);
        gameHUD.interactable = false; // Disable laro habang may tutorial
        ShowStep();
    }

    void ShowStep()
    {
        TutorialStep s = steps[currentStep];
        instructionText.text = s.instruction;

        // I-move ang "Butas" sa position ng Target UI
        if (s.targetUI != null)
        {
            spotlightHole.position = s.targetUI.position;
            spotlightHole.sizeDelta = s.holeSize;
        }
    }

    public void NextStep()
    {
        currentStep++;
        if (currentStep < steps.Length)
        {
            ShowStep();
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        overlayPanel.SetActive(false);
        gameHUD.interactable = true;
        gameHUD.blocksRaycasts = true;
    }
}