using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCard : MonoBehaviour
{
    public Image animalIcon;
    public TextMeshProUGUI titleText;
    
    [Header("Main Button (Rescue1)")]
    public Button acceptBtn; // I-drag dito ang Button object sa Inspector

    public void Setup(QuestInfo info, NPC npc, bool canAccept)
    {
        // I-set ang UI base sa ScriptableObject
        if (titleText != null) titleText.text = info.questTitle; 
        if (animalIcon != null) animalIcon.sprite = info.animalIcon;

        // I-set ang button interaction at listener
        if (acceptBtn != null)
        {
            acceptBtn.interactable = canAccept; 
            acceptBtn.onClick.RemoveAllListeners(); // Importante para iwas double-click bug
            acceptBtn.onClick.AddListener(() => {
                Debug.Log($"Mission Accepted: {info.targetAnimalName}");
                npc.AcceptMission(info); // Tinatawag ang mission logic sa NPC
            });
        }
    }
}