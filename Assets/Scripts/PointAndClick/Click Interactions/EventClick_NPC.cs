using UnityEngine;

public class NPCClickEventData : ClickEventData
{
    public string NPCName;
}

public class EventClick_NPC : EventClick
{
    [SerializeField] private string npcName = "NPC";

    protected override ClickEventData CreateEventData()
    {
        return new NPCClickEventData
        {
            NPCName = npcName,
            ObjectTransform = transform,
            Source = gameObject
        };
    }
}
