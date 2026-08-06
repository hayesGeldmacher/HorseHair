using System;
using UnityEngine;

[System.Serializable]
public class TaskItem
{
    public EventClick_GoalItem goalItem;
    public EventClick_FinalPointBase finalPoint;
    public string GoalText;
    public Camera_Environment startingPosition;
    public DialogueStorage ThoughtText;
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
    [SerializeField] private EventClick_NEI Alternative_NEI;
    [SerializeField] private bool IsGoal;
    public bool Activated = false;
    public bool Talked = false;
    public bool IsUsedByGoal = false;

   

    private void Awake()
    {
        if (Alternative_NEI != null)
        {
            Alternative_NEI.ActivateOrDeactivate(false);
            if (Activated)
                Alternative_NEI.Activated = false;
        }
    }

    protected override void SetType()
    {
        Type = ObjectType.FPB;
        Name = tgName;
    }

    protected override ClickEventData CreateEventData()
    {
        if (IsUsedByGoal)
        {
            return null;
        }

        dialogueText.useAltDialogue = Talked;
        if (!Talked)
        {
            Talked = true;
        }
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
        if (Activated)
        {
            base.ActivateOrDeactivate(activate);
            if (Talked && IsGoal)
                base.ActivateOrDeactivate(false);
        }
        else
        {
            if (Alternative_NEI != null)
                Alternative_NEI.ActivateOrDeactivate(activate);
            base.ActivateOrDeactivate(false);
        }
    }

    public void ChangeTaskStatus(bool status)
    {
        gameObject.SetActive(status);
        if (status)
        {
            task.goalItem.ActivateItems();
            if (IsGoal)
                gameObject.SetActive(false);
        }
    }

    public void ChangeGoalStatus(bool status)
    {
        if (status)
        {
            task.goalItem.StartTask();
            if (IsGoal)
                task.goalItem.ActivateOrDeactivate(true);
        }
    }
}
