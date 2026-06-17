using UnityEngine;

public class AnimatedItemClickEventData : ClickEventData
{
    public string AIName;
}

public class EventClick_AnimatedItems : EventClick
{
    [SerializeField] private string aiName = "Storage";
    [SerializeField] private BoxCollider collisions;
    [SerializeField] private Animator animator;
    [SerializeField] private string OpenString;
    [SerializeField] private string CloseString;

    protected override void SetType()
    {
        Type = ObjectType.AI;
        Name = aiName;
    }

    protected override ClickEventData CreateEventData()
    {
        animator.SetTrigger(OpenString);
        collisions.enabled = false;
        return new AnimatedItemClickEventData
        {
            AIName = aiName,
            ObjectTransform = transform,
            Source = gameObject,
            Description = description,
        };
    }
}
