using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCard : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject acceptButtonObj;    
    public GameObject successImageObj;    
    public GameObject timerButtonObj;     
    public TextMeshProUGUI timerText;     

    [Header("Display Info")]
    public Image animalIcon;
    public TextMeshProUGUI titleText;

    private QuestInfo currentInfo;
    private NPC currentNpc;
    private float cooldownTimer = 0f;
    private bool isCooldown = false;

    public void Setup(QuestInfo info, NPC npc, bool canAccept)
    {
        currentInfo = info;
        currentNpc = npc;

        if (titleText != null) titleText.text = info.questTitle; 
        if (animalIcon != null) animalIcon.sprite = info.animalIcon;

        Button btn = acceptButtonObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                DialogSystem.Instance.OpenMissionPreview(currentInfo, currentNpc);
            });
        }

        RefreshUI();
    }

    void Update()
    {
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            
            if (timerText != null) 
            {
                // Smart Formatting: Tinatawag ang bagong logic natin sa baba
                timerText.text = FormatTime(cooldownTimer);
            }

            if (cooldownTimer <= 0)
            {
                isCooldown = false;
                RefreshUI(); 
            }
        }
    }

    // --- SMART FORMATTING LOGIC ---
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        if (minutes > 0)
        {
            // Kapag 1 minute pataas, ipakita ang "1m 30s"
            return string.Format("{0}m {1:00}s", minutes, seconds);
        }
        else
        {
            // Kapag seconds na lang, ipakita ang "30s" (wala na yung 0m)
            return string.Format("{0}s", seconds);
        }
    }

    public void RefreshUI()
    {
        bool isFinished = PlayerPrefs.GetInt("Mission_" + currentInfo.targetAnimalName, 0) == 1;

        acceptButtonObj.SetActive(false);
        successImageObj.SetActive(false);
        timerButtonObj.SetActive(false);

        if (isFinished)
        {
            successImageObj.SetActive(true);
        }
        else if (isCooldown)
        {
            timerButtonObj.SetActive(true);
            
            Image timerImg = timerButtonObj.GetComponent<Image>();
            if (timerImg != null)
            {
                // Solid grayish color (0.85 alpha)
                timerImg.color = new Color(0.3f, 0.3f, 0.3f, 0.85f);
            }

            Button timerBtn = timerButtonObj.GetComponent<Button>();
            if (timerBtn != null) timerBtn.interactable = false;
        }
        else
        {
            acceptButtonObj.SetActive(true);
        }
    }

    public void StartCooldown(float seconds)
    {
        cooldownTimer = seconds;
        isCooldown = true;
        RefreshUI();
    }
}