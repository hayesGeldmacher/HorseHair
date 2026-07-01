using UnityEngine;

public class NEIClickEventData : ClickEventData
{
    public string NEIName;
}

public class EventClick_NEI : EventClick
{
    [SerializeField] private string neiName = "NEI";
    public bool Activated = true;

    protected override void SetType()
    {
        Type = ObjectType.NEI;
        Name = neiName;
    }

    protected override ClickEventData CreateEventData()
    {
        return new NEIClickEventData
        {
            NEIName = neiName,
            ObjectTransform = transform,
            Source = gameObject,
            Description = description,
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
