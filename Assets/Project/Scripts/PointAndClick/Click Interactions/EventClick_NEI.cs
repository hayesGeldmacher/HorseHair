using UnityEngine;

public class NPCClickEventData : ClickEventData
{
    public string NPCName;
}

public class EventClick_NEI : EventClick
{
    [SerializeField] private string propName = "Prop";

    protected override void SetType()
    {
        Type = ObjectType.NEI;
    }

    protected override ClickEventData CreateEventData()
    {
        return new NPCClickEventData
        {
            NPCName = propName,
            ObjectTransform = transform,
            Source = gameObject
        };
    }
}
