using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestCard : MonoBehaviour
{
    public Image animalIcon;
    public TextMeshProUGUI titleText;
    
    [Header("Button Settings")]
    public Button acceptBtn; 
    public Image buttonImage; // I-drag dito ang Image component ng Accept Button
    public TextMeshProUGUI buttonText; // I-drag dito ang Text child ng Button

    [Header("Sprites")]
    public Sprite successSprite;
    public Sprite defaultSprite;

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

        RefreshUI();

        if (acceptBtn != null)
        {
            acceptBtn.onClick.RemoveAllListeners();
            acceptBtn.onClick.AddListener(() => {
                // STEP 2: Pag-click, bubuksan muna ang Preview Panel bago ang Dialog
                DialogSystem.Instance.OpenMissionPreview(currentInfo, currentNpc);
            });
        }
    }

    void Update()
    {
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isCooldown = false;
                RefreshUI();
            }
            else
            {
                // I-update ang countdown text sa button
                buttonText.text = Mathf.Ceil(cooldownTimer).ToString() + "s";
            }
        }
    }

    public void RefreshUI()
    {
        // I-check kung tapos na ang mission (Success)
        bool isFinished = PlayerPrefs.GetInt("Mission_" + currentInfo.targetAnimalName, 0) == 1;

        if (isFinished)
        {
            buttonImage.sprite = successSprite;
            acceptBtn.interactable = false;
            buttonText.text = "FINISHED";
        }
        else if (isCooldown)
        {
            acceptBtn.interactable = false;
            // Ang text ay ina-update sa Update() function
        }
        else
        {
            buttonImage.sprite = defaultSprite;
            acceptBtn.interactable = true;
            buttonText.text = "RESCUE";
        }
    }

    // Tawagin ito mula sa mission logic mo pag nag-fail
    public void StartCooldown(float seconds)
    {
        cooldownTimer = seconds;
        isCooldown = true;
        acceptBtn.interactable = false;
    }
}