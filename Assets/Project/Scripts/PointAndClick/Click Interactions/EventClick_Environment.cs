using System;
using UnityEngine;

public class TeleportClickEventData : ClickEventData
{
    public string EnvironmentName;
    public float PitchClamp;
    public float YawClamp;
    public float FollowSpeedX;
    public float FollowSpeedY;
    public bool spin_360_x;
    public bool spin_360_y;
    public Camera_Environment Camera;
    public EventClick_Environment source;

    public bool endingCamera;
    public float delayBeforeEnding;
    public DialogueStorage endingDialogue;
    public string NextScene;

    public bool canEnter = true;
    public string requiredItemDesc;

    public bool ActivateFlashlight = false;

    public DialogueStorage movementDialogue;
}

[Serializable]
public class DialogueMovement
{
    public TimeCheck time;
    public DialogueStorage dialogue;
}

public class EventClick_Environment : EventClick
{
    [SerializeField] private Camera_Environment connectedCamera;
    [Header("Required Item")]
    [SerializeField] private EventClick_Item requiredItems;
    [SerializeField] private string requiredItemString;
    [SerializeField] private Inventory playerInventory;
    [Header("Task Limitter")]
    [SerializeField] private EventClick_TaskGiver taskLimit;
    [SerializeField] private PlayerController_PointAndClick playerController;
    [Header("Ending Camera")]
    public bool EndingCamera = false;

    protected override void SetType()
    {
        Type = ObjectType.Environment;
        Name = connectedCamera.environmentName;
    }

    protected override ClickEventData CreateEventData()
    {
        connectedCamera.SetUpEventData(this);
        if (requiredItems != null)
        {
            connectedCamera.TeleportClickEventData.requiredItemDesc = requiredItemString;
            connectedCamera.TeleportClickEventData.canEnter = 
                playerInventory.CheckItemInInventory(requiredItems.itemName);
            connectedCamera.TeleportClickEventData.DialogueText = dialogueText;
        }
        connectedCamera.talked = true;
        return connectedCamera.TeleportClickEventData;
    }

    public override void ActivateOrDeactivate(bool activate)
    {
        if (taskLimit != null)
        {
            if (playerController.currentTask != taskLimit.task)
            {
                this.gameObject.SetActive(false);
                return;
            }
        }
        this.gameObject.SetActive(activate);
    }
}
