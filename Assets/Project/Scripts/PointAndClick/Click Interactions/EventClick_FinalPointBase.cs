using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPBClickEventData : ClickEventData
{
    public string FPBName;
    public bool GoToNextScene;
    public string CompleteString;
    public string IncompleteString;
}

public class EventClick_FinalPointBase : EventClick
{
    [SerializeField] private string fpbName = "FPB";
    [SerializeField] private string NotCompletedText = "Goal Not Completed";
    [SerializeField] private string CompletedText = "Goal Complete";
   

    private bool goToNextScene = false;
    public bool Activated = false;

    private void OnEnable()
    {
        EventClick_GoalItem.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        EventClick_GoalItem.GoalCompleted -= HandleGoalCompleted;
    }

    protected override void SetType()
    {
        Type = ObjectType.FPB;
        Name = fpbName;
    }

    protected override ClickEventData CreateEventData()
    {
        if (goToNextScene)
        {
            description = CompletedText;
        }
        else
        {
            description = NotCompletedText;
        }

        return new FPBClickEventData
        {
            FPBName = fpbName,
            ObjectTransform = transform,
            Source = gameObject,
            Description = description,
            DialogueText = dialogueText,
            GoToNextScene = goToNextScene,
            CompleteString = CompletedText,
            IncompleteString = NotCompletedText,
        };
    }

    private void HandleGoalCompleted(GoalCompletionData data)
    {
        goToNextScene = data.IsCompleted;
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        if (!Activated)
            base.ActivateOrDeactivate(false);
        else
            base.ActivateOrDeactivate(activate);
    }
}
    