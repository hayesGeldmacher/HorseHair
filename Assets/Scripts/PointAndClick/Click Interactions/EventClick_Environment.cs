using UnityEngine;

public class TeleportClickEventData : ClickEventData
{
    public string EnvironmentName;
    public float PitchClamp;
    public float YawClamp;
    public float FollowSpeedX;
    public float FollowSpeedY;
    public Camera_Environment Camera;
}   

public class EventClick_Environment : EventClick
{
    [SerializeField] private Camera_Environment connectedCamera;

    protected override void SetType()
    {
        Type = ObjectType.Environment;
    }

    protected override ClickEventData CreateEventData()
    {
        if (connectedCamera.TeleportClickEventData == null)
        {
            connectedCamera.SetUpEventData();
        }
        return connectedCamera.TeleportClickEventData;
    }
}
