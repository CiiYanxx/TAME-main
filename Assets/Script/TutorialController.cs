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

    private Coroutine arrowAnim;

    private bool arrowsLocked = false;

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
    // STATIC ARROW SYSTEM
    // =========================

    public void ShowArrowOnIndex(int index)
    {
        if (arrowsLocked)
        {
            Debug.Log("[Tutorial] Arrow blocked (locked)");
            return;
        }

        HideArrowUI();

        if (arrowObjects == null || index >= arrowObjects.Length)
        {
            Debug.LogWarning("[Tutorial] Invalid arrow index: " + index);
            return;
        }

        GameObject arrow = arrowObjects[index];
        if (arrow == null)
        {
            Debug.LogWarning("[Tutorial] Arrow is NULL at index " + index);
            return;
        }

        Debug.Log("[Tutorial] STATIC ARROW SPAWN → " + arrow.name);

        arrow.SetActive(true);

        RectTransform rect = arrow.GetComponent<RectTransform>();

        if (rect != null)
            arrowAnim = StartCoroutine(BounceArrow(rect));
    }

    IEnumerator BounceArrow(RectTransform arrow)
    {
        Vector2 startPos = arrow.anchoredPosition;

        while (true)
        {
            if (arrow == null) yield break;

            float bounce = Mathf.Sin(Time.unscaledTime * 5f) * 10f;
            arrow.anchoredPosition = startPos + new Vector2(0, bounce);

            yield return null;
        }
    }

    // =========================
    // RUNTIME ARROWS (QUESTCARD)
    // =========================

    public void RegisterRuntimeArrow(GameObject obj)
    {
        if (obj == null) return;

        if (!runtimeArrows.Contains(obj))
        {
            runtimeArrows.Add(obj);
            Debug.Log("[Tutorial] REGISTER runtime arrow → " + obj.name);
        }
    }

    public void UnregisterRuntimeArrow(GameObject obj)
    {
        if (runtimeArrows.Contains(obj))
        {
            runtimeArrows.Remove(obj);
            Debug.Log("[Tutorial] UNREGISTER runtime arrow → " + obj.name);
        }
    }

    public void HideArrowUI()
    {
        Debug.Log("[Tutorial] HideArrowUI CALLED");

        if (arrowAnim != null)
        {
            StopCoroutine(arrowAnim);
            arrowAnim = null;
        }

        if (arrowObjects != null)
        {
            foreach (var a in arrowObjects)
            {
                if (a != null && a.activeSelf)
                {
                    Debug.Log("[Tutorial] STATIC OFF → " + a.name);
                    a.SetActive(false);
                }
            }
        }

        for (int i = 0; i < runtimeArrows.Count; i++)
        {
            if (runtimeArrows[i] != null)
            {
                Debug.Log("[Tutorial] RUNTIME OFF → " + runtimeArrows[i].name);
                runtimeArrows[i].SetActive(false);
            }
        }

        runtimeArrows.Clear();
    }

    public void OnConversationEnd()
    {
        Debug.Log("[Tutorial] Conversation ended cleanup");

        arrowsLocked = true;

        HideArrowUI();

        runtimeArrows.Clear();
    }

    public void HardCleanupAllArrows()
    {
        Debug.Log("[Tutorial] HARD CLEANUP TRIGGERED");

        HideArrowUI();

        runtimeArrows.Clear();

        arrowsLocked = true;

        Debug.Log("[Tutorial] HARD CLEANUP DONE");
    }
}