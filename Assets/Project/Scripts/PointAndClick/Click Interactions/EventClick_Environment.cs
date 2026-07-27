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
}   

public class EventClick_Environment : EventClick
{
    [SerializeField] private Camera_Environment connectedCamera;
    public bool EndingCamera = false;

    protected override void SetType()
    {
        Type = ObjectType.Environment;
        Name = connectedCamera.environmentName;
    }

    protected override ClickEventData CreateEventData()
    {
        connectedCamera.SetUpEventData(this);
        return connectedCamera.TeleportClickEventData;
    }
}
