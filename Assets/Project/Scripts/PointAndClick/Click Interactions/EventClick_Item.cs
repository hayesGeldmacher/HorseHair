using UnityEngine;
public class ItemClickEventData : ClickEventData
{
    public string ItemName;
    public EventClick_Item SourceItem;
    public Sprite ItemImage;
}

public class EventClick_Item : EventClick
{
    [SerializeField] public string itemName = "Item";
    [SerializeField] public Sprite itemImage;
    public bool Collected = false;
    public bool Activated = false;

    [Header("Bird-Specific")]
    public bool disableMesh = false;
    public MeshRenderDisappear meshRender;

    protected override void SetType()
    {
        Type = ObjectType.Item;
        Name = itemName;
    }

    protected override ClickEventData CreateEventData()
    {
        CollectedByPlayer();
        return new ItemClickEventData
        {
            ItemName = itemName,
            ObjectTransform = transform,
            Source = gameObject,
            SourceItem = this,
            Description = description,
            ItemImage = itemImage,
        };
    }

    private void CollectedByPlayer()
    {
        Collected = true;
        if (disableMesh && meshRender)
        {
            meshRender.DisableMesh();
        }
        gameObject.SetActive(false);
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        if (Collected)
            return;
        if (!Activated)
            base.ActivateOrDeactivate(false);
        else
            base.ActivateOrDeactivate(activate);
    }
}
