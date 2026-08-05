using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalEventData : ClickEventData
{
    public string GoalName;
    public EventClick_GoalItem SourceGoal;
}

public class GoalCompletionData
{
    public string GoalName;
    public Dictionary<EventClick_Item, bool> NeededItems;
    public EventClick_GoalItem SourceGoal;
    public bool IsCompleted;
    public string CompletedString;
    public string NotCompletedString;
    public string nextTask;
    public EventClick_TaskGiver sequenceTask;
}

public class EventClick_GoalItem : EventClick
{
    [SerializeField] private string goalName = "Goal Item";
    [SerializeField] private EventClick_Item[] requiredItems;
    [SerializeField] private string NotCompletedGoalString = "I still need ";
    [SerializeField] private string CompletedGoalString = "Perfect, now I should go to school";
    [SerializeField] private string TaskAfterComplettion = "I should go to school now";
    [Header("Next Goal")]
    [SerializeField] private EventClick_TaskGiver nextTask;

    private Dictionary<EventClick_Item, bool> itemCollectionStatus = 
        new Dictionary<EventClick_Item, bool>();
    public static event System.Action<GoalCompletionData> GoalCompleted;
    public bool Activated = false;

    private void Awake()
    {
        if (nextTask)
        {
            nextTask.IsUsedByGoal = true;
        }
    }

    public void StartTask()
    {
        Activated = true;
        foreach (var item in requiredItems)
        {
            item.Activated = true;
        }
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        if (!Activated)
        {
            activate = false;
        }
        this.gameObject.SetActive(activate);
        //foreach (var item in requiredItems)
        //{
        //    item.ActivateOrDeactivate(activate);
        //}
    }

    protected override void SetType()
    {
        Type = ObjectType.Goal;
        foreach(var item in requiredItems)
        {
            itemCollectionStatus[item] = false;
        }
        Name = goalName;
    }

    protected override ClickEventData CreateEventData()
    {
        return new GoalEventData
        {
            GoalName = goalName,
            ObjectTransform = transform,
            Source = gameObject,
            SourceGoal = this,
            Description = description,
        };
    }

    public void CheckGoal()
    {
        bool allCollected = !itemCollectionStatus.ContainsValue(false);
        if (GoalCompleted != null)
        {
            string neededItems = NotCompletedGoalString;
            List<string> listOfItems = new List<string>();
            foreach (var item in itemCollectionStatus)
            {
                if (!item.Value)
                {
                    listOfItems.Add(item.Key.itemName);
                }
            }
            if (listOfItems.Count > 0)
            {
                for (int i = 0; i < listOfItems.Count - 1; i++)
                {
                    neededItems += listOfItems[i] + ", ";
                }
                neededItems += listOfItems[listOfItems.Count - 1] + ".";
            }

            GoalCompleted.Invoke(new GoalCompletionData
            {
                GoalName = goalName,
                NeededItems = new Dictionary<EventClick_Item, bool>(itemCollectionStatus),
                SourceGoal = this,
                IsCompleted = allCollected,
                CompletedString = CompletedGoalString,
                NotCompletedString = neededItems,
                nextTask = TaskAfterComplettion,
                sequenceTask = nextTask,
            });
        }
    }

    public bool CollectItem(EventClick_Item item)
    {
        if (itemCollectionStatus.ContainsKey(item))
        {
            itemCollectionStatus[item] = true;
            return true;
        }
        return false;
    }

    public void ActivateItems()
    {
        foreach(var item in requiredItems)
        {
            item.ActivateOrDeactivate(true);
        }
    }
}
