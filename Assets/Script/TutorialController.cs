using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    public GameObject overlay;
    public GameObject[] panels;

    [Header("Multi-Target Arrow System")]
    public RectTransform dialogArrow; 
    public RectTransform[] targetButtons; 

    [Header("Arrow Animation Settings")]
    public Vector3 arrowOffset = new Vector3(0, 150, 0); 
    public float bounceDistance = 20f;
    public float bounceSpeed = 5f;

    private const string TUTORIAL_KEY = "Tutorial_Completed";
    private int currentIndex = -1;
    private bool[] stepTriggered;
    private bool waitingForContinue = false;
    private bool readyForNextStep = true;

    private RectTransform currentTarget;
    private Coroutine animationCoroutine;

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
        HideArrowUI();
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
        yield return new WaitForSecondsRealtime(1f);
        HideAll();
        currentIndex = index;

        if (overlay != null) overlay.SetActive(true);
        if (panels[index] != null) panels[index].SetActive(true);

        Time.timeScale = 0f;
        if (PlayerMovement.Instance != null) PlayerMovement.Instance.canControl = false;

        waitingForContinue = true;
    }

    public void Continue()
    {
        if (!waitingForContinue) return;

        HideAll();
        Time.timeScale = 1f;

        if (PlayerMovement.Instance != null) PlayerMovement.Instance.canControl = true;

        waitingForContinue = false;
        readyForNextStep = true;
    }

    public void Skip() => CompleteTutorial();

    void CompleteTutorial()
    {
        HideAll();
        HideArrowUI();

        Time.timeScale = 1f;
        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();

        if (PlayerMovement.Instance != null) PlayerMovement.Instance.canControl = true;

        gameObject.SetActive(false);
    }

    public void Tutorial0_Joystick() => Show(0);
    public void Tutorial1_Swipe() { if (stepTriggered[0]) Show(1); }
    public void Tutorial2_Interact() { if (stepTriggered[1]) Show(2); }

    // =========================
    // FIXED ARROW SYSTEM
    // =========================

    public void ShowArrowOnIndex(int index)
    {
        if (dialogArrow == null || targetButtons == null || index >= targetButtons.Length)
        {
            Debug.LogWarning("Invalid arrow index!");
            return;
        }

        if (targetButtons[index] == null) return;

        StopArrowAnimation();

        Canvas.ForceUpdateCanvases(); // VERY IMPORTANT FIX

        currentTarget = targetButtons[index];
        dialogArrow.gameObject.SetActive(true);
        dialogArrow.position = currentTarget.position + arrowOffset;

        animationCoroutine = StartCoroutine(AnimateArrow());
    }

    IEnumerator AnimateArrow()
    {
        while (true)
        {
            if (currentTarget != null)
            {
                Vector3 basePos = currentTarget.position + arrowOffset;

                float newY = basePos.y + Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceDistance;

                dialogArrow.position = new Vector3(basePos.x, newY, basePos.z);
            }

            yield return null;
        }
    }

    public void HideArrowUI()
    {
        StopArrowAnimation();
        currentTarget = null;

        if (dialogArrow != null)
            dialogArrow.gameObject.SetActive(false);
    }

    void StopArrowAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
}