using System.Collections.Generic;
using UnityEngine;

public class NEIClickEventData : ClickEventData
{
    public string NEIName;
}

//[System.Serializable]
//public class NEITimeSetting
//{
//    public TimeOfDay timeOfDay;
//    public int TaskNum;
//}

public class EventClick_NEI : EventClick
{
    [SerializeField] private string neiName = "NEI";
    [SerializeField] private bool dieAfterClick = false;
    public bool Activated = true;

    //[Header("NEI Settings")]
    //[SerializeField] private bool AvailableEveryDay = true;
    //[SerializeField] private List<NEITimeSetting> activeTimeOfDays = new List<NEITimeSetting>();

    //private void Awake()
    //{
    //    if (!AvailableEveryDay)
    //    {
    //        bool isActive = false;
    //        TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)TimeOfDay.Morning);
    //        int currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
    //        foreach (var timeSetting in activeTimeOfDays)
    //        {
    //            if (timeSetting.timeOfDay == currentTimeOfDay && timeSetting.TaskNum == currentTaskNum)
    //            {
    //                isActive = true;
    //                break;
    //            }
    //        }
    //        Activated = isActive;
    //    }
    //}

    protected override void SetType()
    {
        Type = ObjectType.NEI;
        Name = neiName;
    }

    protected override ClickEventData CreateEventData()
    {
        if (dieAfterClick)
        {
            Activated = false;
            ActivateOrDeactivate(false);
        }
        return new NEIClickEventData
        {
            NEIName = neiName,
            ObjectTransform = transform,
            Source = gameObject,
            Description = description,
            DialogueText = dialogueText,
        };
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        if (Activated)
        {
            base.ActivateOrDeactivate(activate);
        }
        else
        {
            base.ActivateOrDeactivate(false);
        }
    }
}
