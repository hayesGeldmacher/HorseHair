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
}

public class EventClick_GoalItem : EventClick
{
    [SerializeField] private string goalName = "Goal Item";
    [SerializeField] private EventClick_Item[] requiredItems;

    private Dictionary<EventClick_Item, bool> itemCollectionStatus = 
        new Dictionary<EventClick_Item, bool>();
    public static event System.Action<GoalCompletionData> GoalCompleted;

    protected override void SetType()
    {
        Type = ObjectType.Goal;
        foreach(var item in requiredItems)
        {
            itemCollectionStatus[item] = false;
        }
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
            GoalCompleted.Invoke(new GoalCompletionData
            {
                GoalName = goalName,
                NeededItems = new Dictionary<EventClick_Item, bool>(itemCollectionStatus),
                SourceGoal = this,
                IsCompleted = allCollected
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
}
