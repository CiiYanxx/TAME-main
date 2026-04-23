using System.Collections;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    public GameObject overlay;
    public GameObject[] panels; // 0=Joystick, 1=Swipe, 2=Interact, 3=Dialog, 4=Location, 5=Animal, 6=Arrow

    private const string TUTORIAL_KEY = "Tutorial_Completed";

    private int currentIndex = -1;
    private bool[] stepTriggered;
    private bool waitingForContinue = false;
    private bool readyForNextStep = true;

    void Awake()
    {
        Instance = this;

        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        stepTriggered = new bool[panels.Length];
        HideAll();
    }

    void HideAll()
    {
        if (overlay != null) overlay.SetActive(false);

        foreach (var p in panels)
        {
            if (p != null) p.SetActive(false);
        }
    }

    void Show(int index)
    {
        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1) return;
        if (index < 0 || index >= panels.Length) return;
        if (stepTriggered[index]) return;
        if (!readyForNextStep) return;

        stepTriggered[index] = true;
        readyForNextStep = false;

        StartCoroutine(ShowStep(index));
    }

    IEnumerator ShowStep(int index)
    {
        // REQUIRED: delay bago lumabas step UI
        yield return new WaitForSecondsRealtime(1f);

        HideAll();
        currentIndex = index;

        if (overlay != null) overlay.SetActive(true);
        if (panels[index] != null) panels[index].SetActive(true);

        Time.timeScale = 0f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = false;

        waitingForContinue = true;
    }

    // =========================
    // CONTINUE BUTTON (UPDATED)
    // =========================
    public void Continue()
    {
        if (!waitingForContinue) return;

        HideAll();
        Time.timeScale = 1f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = true;

        waitingForContinue = false;

        // IMPORTANT:
        // hindi na siya mag-aadvance dito
        // hintay na lang ng game event (movement / interact / etc.)
        readyForNextStep = true;
    }

    public void Skip()
    {
        CompleteTutorial();
    }

    void CompleteTutorial()
    {
        HideAll();
        Time.timeScale = 1f;

        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = true;

        gameObject.SetActive(false);
    }

    // =========================
    // TRIGGERS
    // =========================
    public void Tutorial0_Joystick()
    {
        Show(0);
    }

    public void Tutorial1_Swipe()
    {
        if (!stepTriggered[0]) return;
        Show(1);
    }

    public void Tutorial2_Interact()
    {
        if (!stepTriggered[1]) return;
        Show(2);
    }

    public void ShowArrow(int index)
    {
        if (index >= 3 && index < panels.Length)
        {
            Show(index);
        }
    }

    public bool IsBasicTutorialDone()
    {
        return stepTriggered[0] && stepTriggered[1] && stepTriggered[2];
    }

    public void ForceComplete()
    {
        CompleteTutorial();
    }
}