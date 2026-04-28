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

    [Header("Pause Game per Tutorial Panel")]
    public bool[] pauseOnPanel;

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
        yield return new WaitForSecondsRealtime(0f);

        ShowPanel(index);

        bool shouldPause = false;

        if (pauseOnPanel != null && index < pauseOnPanel.Length)
            shouldPause = pauseOnPanel[index];

        if (shouldPause)
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

        if (currentIndex == 4)
            StartCoroutine(Tutorial5TrustDelay());
    }

    private void RestorePlayerControls()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canControl = true;
        }

        if (RescueController.Instance != null)
        {
            if (RescueController.Instance.moveJoystick != null)
                RescueController.Instance.moveJoystick.SetActive(true);

            if (RescueController.Instance.lookJoystick != null)
                RescueController.Instance.lookJoystick.SetActive(true);

            var joy = RescueController.Instance.moveJoystick.GetComponent<VirtualJoystick>();

            if (joy != null)
                joy.ResetJoystick();
        }
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
        if (stepTriggered[0])
        {
            Show(1);
            StartCoroutine(Tutorial2Delay());
        }
    }

    IEnumerator Tutorial2Delay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Tutorial2_Interact();
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
        yield return new WaitForSeconds(2.5f);
        Show(3);
    }

    public void Tutorial4_Sneak()
    {
        if (stepTriggered[4]) return;

        if (stepTriggered[3])
            Show(4);
    }

    public void Tutorial5_Trust()
    {
        if (stepTriggered[5]) return;

        if (stepTriggered[4])
            StartCoroutine(Tutorial5TrustDelay());
    }

    IEnumerator Tutorial5TrustDelay()
    {
        yield return new WaitForSecondsRealtime(0f);
        Show(5);
    }

    public void Tutorial6_Feed()
    {
        if (stepTriggered[6]) return;

        if (stepTriggered[5])
            Show(6);
    }

    public void Tutorial7_Tame()
    {
        if (stepTriggered[7]) return;

        if (stepTriggered[6])
            Show(7);
    }

    public void Tutorial8_Minigame()
    {
        if (stepTriggered[8]) return;

        if (stepTriggered[7])
            Show(8);
    }

    // 🔥 NEW TUTORIAL 9 = RESCUE SUCCESS
    public void Tutorial9_Rescue()
    {
        if (stepTriggered[9]) return;

        if (stepTriggered[8])
            Show(9);
    }

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

        // 🔥 ONLY continue tutorial if mission is actually active
        if (PlayerPrefs.GetInt("IsMissionActive", 0) == 1)
        {
            Tutorial3_HintPanel();
        }
    }

    public void HardCleanupAllArrows()
    {
        HideArrowUI();
        runtimeArrows.Clear();
        arrowsLocked = true;
    }

}