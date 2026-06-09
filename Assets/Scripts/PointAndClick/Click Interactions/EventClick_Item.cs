using UnityEngine;
public class ItemClickEventData : ClickEventData
{
    public string ItemName;
}

public class EventClick_Item : EventClick
{
    [SerializeField] private string itemName = "Item";

    protected override void SetType()
    {
        Type = ObjectType.Item;
    }

    protected override ClickEventData CreateEventData()
    {
        return new ItemClickEventData
        {
            ItemName = itemName,
            ObjectTransform = transform,
            Source = gameObject
        };
    }
}
