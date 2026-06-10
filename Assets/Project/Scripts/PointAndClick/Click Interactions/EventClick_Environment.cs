using UnityEngine;

public class TeleportClickEventData : ClickEventData
{
    public string EnvironmentName;
    public float PitchClamp;
    public float YawClamp;
    public float FollowSpeedX;
    public float FollowSpeedY;
}   

public class EventClick_Environment : EventClick
{
    [SerializeField] private string environmentName = "Environment";
    [Header("Camera Settings")]
    [SerializeField, Tooltip("How far can the camera move up and down")]
    private float _pitchClamp = 20f;
    [SerializeField, Tooltip("How far can the camera move left and right")]
    private float _yawClamp = 20f;
    [SerializeField, Tooltip("How fast the camera follows the cursor horizontally")]
    private float _followSpeedX = 2f;
    [SerializeField, Tooltip("How fast the camera follows the cursor vertically")]
    private float _followSpeedY = 2f;

    protected override void SetType()
    {
        Type = ObjectType.Environment;
    }

    protected override ClickEventData CreateEventData()
    {
        return new TeleportClickEventData
        {
            EnvironmentName = environmentName,
            PitchClamp = _pitchClamp,
            YawClamp = _yawClamp,
            FollowSpeedX = _followSpeedX,
            FollowSpeedY = _followSpeedY,   
            ObjectTransform = transform,
            Source = gameObject
        };
    }
}
