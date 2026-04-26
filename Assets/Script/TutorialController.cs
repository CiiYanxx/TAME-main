using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [System.Serializable]
    public class ArrowAnimationSettings
    {
        public enum MoveType
        {
            UpDown,
            LeftRight,
            DiagonalUpRight,
            DiagonalUpLeft,
            Circular,
            None
        }

        public MoveType moveType = MoveType.UpDown;
        public float speed = 5f;
        public float distance = 10f;
        public bool useUnscaledTime = true;
    }

    public GameObject overlay;
    public GameObject[] panels;

    [Header("Arrow UI Objects (Same Index as Panels)")]
    public GameObject[] arrowObjects;

    [Header("Per Arrow Animation Settings")]
    public ArrowAnimationSettings[] arrowSettings;

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
        if (overlay != null)
            overlay.SetActive(false);

        foreach (var p in panels)
            if (p != null)
                p.SetActive(false);

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

    public void Skip()
    {
        CompleteTutorial();
    }

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

    // =========================
    // TUTORIAL FLOW
    // =========================

    public void Tutorial0_Joystick() => Show(0);

    public void Tutorial1_Swipe()
    {
        if (stepTriggered[0]) Show(1);
    }

    public void Tutorial2_Interact()
    {
        if (stepTriggered[1]) Show(2);
    }

    public void Tutorial3_HintPanel()
    {
        if (stepTriggered[2])
            StartCoroutine(ShowHintPanelDelay());
    }

    IEnumerator ShowHintPanelDelay()
    {
        yield return new WaitForSeconds(5f);
        Show(3);
    }

    // =========================
    // ARROW SYSTEM
    // =========================

    public void ShowArrowOnIndex(int index)
    {
        if (arrowsLocked) return;

        HideArrowUI();

        if (arrowObjects == null) return;
        if (index < 0 || index >= arrowObjects.Length) return;

        GameObject arrow = arrowObjects[index];
        if (arrow == null) return;

        arrow.SetActive(true);

        RectTransform rect = arrow.GetComponent<RectTransform>();
        if (rect != null)
            arrowAnim = StartCoroutine(AnimateArrow(rect, index));
    }

    IEnumerator AnimateArrow(RectTransform arrow, int index)
    {
        Vector2 startPos = arrow.anchoredPosition;

        ArrowAnimationSettings settings = null;

        if (arrowSettings != null && index < arrowSettings.Length)
            settings = arrowSettings[index];

        if (settings == null)
            yield break;

        while (true)
        {
            if (arrow == null)
                yield break;

            float t = settings.useUnscaledTime ? Time.unscaledTime : Time.time;
            float wave = Mathf.Sin(t * settings.speed) * settings.distance;

            Vector2 offset = Vector2.zero;

            switch (settings.moveType)
            {
                case ArrowAnimationSettings.MoveType.UpDown:
                    offset = new Vector2(0, wave);
                    break;

                case ArrowAnimationSettings.MoveType.LeftRight:
                    offset = new Vector2(wave, 0);
                    break;

                case ArrowAnimationSettings.MoveType.DiagonalUpRight:
                    offset = new Vector2(wave, wave);
                    break;

                case ArrowAnimationSettings.MoveType.DiagonalUpLeft:
                    offset = new Vector2(-wave, wave);
                    break;

                case ArrowAnimationSettings.MoveType.Circular:
                    offset = new Vector2(
                        Mathf.Cos(t * settings.speed) * settings.distance,
                        Mathf.Sin(t * settings.speed) * settings.distance
                    );
                    break;

                case ArrowAnimationSettings.MoveType.None:
                    offset = Vector2.zero;
                    break;
            }

            arrow.anchoredPosition = startPos + offset;

            yield return null;
        }
    }

    // =========================
    // RUNTIME ARROWS
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

        if (arrowObjects != null)
        {
            foreach (var a in arrowObjects)
                if (a != null)
                    a.SetActive(false);
        }

        for (int i = 0; i < runtimeArrows.Count; i++)
        {
            if (runtimeArrows[i] != null)
                runtimeArrows[i].SetActive(false);
        }

        runtimeArrows.Clear();
    }

    public void OnConversationEnd()
    {
        arrowsLocked = true;

        HideArrowUI();

        runtimeArrows.Clear();

        arrowsLocked = false;

        Tutorial3_HintPanel();
    }

    public void HardCleanupAllArrows()
    {
        HideArrowUI();
        runtimeArrows.Clear();
        arrowsLocked = true;
    }
}