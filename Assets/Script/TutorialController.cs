using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    public GameObject overlay;
    public GameObject[] panels;

    [Header("Multi-Target Arrow System (UI OBJECTS)")]
    public RectTransform[] targetButtons;

    [Header("Arrow UI Objects (ASSIGN PER INDEX, DEFAULT OFF)")]
    public GameObject[] arrowObjects;

    private const string TUTORIAL_KEY = "Tutorial_Completed";

    private int currentIndex = -1;
    private bool[] stepTriggered;
    private bool[] panelArrowShown;

    private bool waitingForContinue = false;
    private bool readyForNextStep = true;

    private RectTransform currentArrow;
    private Coroutine arrowAnim;

    // 🔥 GLOBAL LOCK
    private bool arrowsLocked = false;

    // 🔥 RUNTIME ARROWS (FIX FOR PREFAB ISSUE)
    private List<GameObject> runtimeArrows = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        stepTriggered = new bool[panels.Length];
        panelArrowShown = new bool[panels.Length];

        HideAll();
        HideArrowUI();
    }

    void HideAll()
    {
        if (overlay != null) overlay.SetActive(false);

        foreach (var p in panels)
            if (p != null) p.SetActive(false);

        HideArrowUI();
    }

    // =========================
    // PANEL SYSTEM
    // =========================
    void ShowPanel(int index)
    {
        HideAll();

        if (overlay != null)
            overlay.SetActive(true);

        if (panels[index] != null)
            panels[index].SetActive(true);

        currentIndex = index;

        if (!panelArrowShown[index] && !arrowsLocked)
        {
            panelArrowShown[index] = true;
            ShowArrowOnIndex(index);
        }
    }

    void Show(int index)
    {
        if (arrowsLocked) return;
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

        ShowPanel(index);

        Time.timeScale = 0f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = false;

        waitingForContinue = true;
    }

    public void Continue()
    {
        if (!waitingForContinue) return;

        HideAll();

        Time.timeScale = 1f;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = true;

        waitingForContinue = false;
        readyForNextStep = true;
    }

    public void Skip() => CompleteTutorial();

    void CompleteTutorial()
    {
        arrowsLocked = true;

        HideAll();

        Time.timeScale = 1f;

        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.canControl = true;

        gameObject.SetActive(false);
    }

    public void Tutorial0_Joystick() => Show(0);
    public void Tutorial1_Swipe() { if (stepTriggered[0]) Show(1); }
    public void Tutorial2_Interact() { if (stepTriggered[1]) Show(2); }

    // =========================
    // STATIC ARROWS
    // =========================
    public void ShowArrowOnIndex(int index)
    {
        if (arrowsLocked) return;

        HideArrowUI();

        if (arrowObjects == null || index >= arrowObjects.Length) return;

        GameObject arrow = arrowObjects[index];
        if (arrow == null) return;

        arrow.SetActive(true);

        currentArrow = arrow.GetComponent<RectTransform>();

        if (currentArrow != null)
            arrowAnim = StartCoroutine(BounceArrow(currentArrow));
    }

    IEnumerator BounceArrow(RectTransform arrow)
    {
        Vector2 startPos = arrow.anchoredPosition;

        while (true)
        {
            // 🔥 FIX: check if destroyed
            if (arrow == null)
            {
                Debug.Log("[Tutorial] Arrow destroyed → stopping coroutine");
                yield break;
            }

            float bounce = Mathf.Sin(Time.unscaledTime * 5f) * 10f;
            arrow.anchoredPosition = startPos + new Vector2(0, bounce);

            yield return null;
        }
    }

    // =========================
    // 🔥 RUNTIME ARROWS FIX (QUESTCARD PREFABS)
    // =========================
    public void RegisterRuntimeArrow(GameObject obj)
    {
        if (obj == null) return;

        if (!runtimeArrows.Contains(obj))
            runtimeArrows.Add(obj);
    }

    public void UnregisterRuntimeArrow(GameObject obj)
    {
        if (runtimeArrows.Contains(obj))
            runtimeArrows.Remove(obj);
    }

    public void HideArrowUI()
    {
        if (arrowAnim != null)
        {
            StopCoroutine(arrowAnim);
            arrowAnim = null;
        }

        currentArrow = null;

        if (arrowObjects == null) return;

        foreach (var a in arrowObjects)
        {
            if (a != null)
                a.SetActive(false);
        }
    }
   
    public void OnConversationEnd()
    {
        arrowsLocked = true;
        HideArrowUI();
    }

    public void ForceHideAndReset()
    {
        HideArrowUI();
        currentIndex = -1;
    }

    public void ResetArrowsOnConversationEnd()
    {
        OnConversationEnd();
    }

    public void HardCleanupAllArrows()
    {
        Debug.Log("[Tutorial] HARD CLEANUP TRIGGERED");

        // 1. static arrows
        if (arrowObjects != null)
        {
            foreach (var a in arrowObjects)
            {
                if (a != null)
                {
                    a.SetActive(false);
                    Debug.Log("[Tutorial] Static arrow OFF → " + a.name);
                }
            }
        }

        // 2. runtime arrows (REAL FIX)
        foreach (var obj in runtimeArrows)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log("[Tutorial] Runtime arrow OFF → " + obj.name);
            }
        }

        runtimeArrows.Clear();

        // ❌ REMOVE THIS COMPLETELY (ITO SUMISIRA)
        // FindObjectsOfType + Contains("row")
    }

    public void DisableAllQuestCardArrows()
    {
        QuestCard[] cards = FindObjectsOfType<QuestCard>();

        foreach (var card in cards)
        {
            if (card != null)
            {
                card.DisableArrow();
            }
        }

        runtimeArrows.Clear();

        Debug.Log("[Tutorial] ALL QuestCard arrows disabled cleanly");
    }
    }