using UnityEngine;

[System.Serializable]
public class Quest
{
    public QuestInfo info; 

    [Header("Quest Status")]
    public bool initialDialogCompleted = false;
    public bool accepted = false;
    public bool declined = false;
    public bool isCompleted = false;
    public bool isMissionSuccess = false; 

    public Quest(QuestInfo questInfo)
    {
        info = questInfo;
    }
}