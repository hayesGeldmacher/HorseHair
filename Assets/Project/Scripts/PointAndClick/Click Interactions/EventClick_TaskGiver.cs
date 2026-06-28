using System;
using UnityEngine;

[System.Serializable]
public class TaskItem
{
    public EventClick_GoalItem goalItem;
    public EventClick_FinalPointBase finalPoint;
    public string GoalText;
    public Camera_Environment startingPosition;
    public string ThoughtText;
}

public class TaskClickEventData : ClickEventData
{
    public string TaskName;
    public TaskItem Task;
    public EventClick_TaskGiver Giver;
}

public class EventClick_TaskGiver : EventClick
{
    [SerializeField] private string tgName = "TG";
    [SerializeField] public TaskItem task;

    protected override void SetType()
    {
        Type = ObjectType.FPB;
        Name = tgName;
    }

    protected override ClickEventData CreateEventData()
    {
        return new TaskClickEventData
        {
            TaskName = tgName,
            ObjectTransform = transform,
            Source = gameObject,
            Description = description,
            DialogueText = dialogueText,
            Task = task,
            Giver = this,
        };
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        ChangeGoalStatus(activate);
        ChangeTaskStatus(activate);
    }

    public void ChangeTaskStatus(bool status)
    {
        gameObject.SetActive(status);
    }

    public void ChangeGoalStatus(bool status)
    {
        task.goalItem.ActivateOrDeactivate(status);
        task.finalPoint.ActivateOrDeactivate(status);
    }
}
