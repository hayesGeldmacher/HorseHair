using Unity.VisualScripting;
using UnityEngine;

public class Camera_Environment : MonoBehaviour
{
    [SerializeField] public string environmentName = "Environment";
    [Header("Camera Settings")]
    [SerializeField, Tooltip("How far can the camera move up and down")]
    private float _pitchClamp = 20f;
    [SerializeField, Tooltip("How far can the camera move left and right")]
    private float _yawClamp = 20f;
    [SerializeField, Tooltip("How fast the camera follows the cursor horizontally")]
    private float _followSpeedX = 2f;
    [SerializeField, Tooltip("How fast the camera follows the cursor vertically")]
    private float _followSpeedY = 2f;
    [SerializeField]
    private EventClick_Environment selfClickEvent;
    [SerializeField] private GameObject _arrows;

    private GameObject Arrows;
    public TeleportClickEventData TeleportClickEventData;
    private EventClick_Environment[] connectedClickEvents;

    private void Start()
    {
        if (TeleportClickEventData == null)
            SetUpEventData();

        connectedClickEvents = _arrows.GetComponentsInChildren<EventClick_Environment>();
        ActivateOrDeactivate(false);
    }
    public void TeleportToSelf()
    {
        selfClickEvent.ForceClick();
    }

    public void ActivateOrDeactivate(bool State)
    {
        gameObject.SetActive(State);  
        foreach (var clickEvent in connectedClickEvents)
        {
            clickEvent.gameObject.SetActive(State);
        }
    }

    public void SetUpEventData()
    {
        TeleportClickEventData = new TeleportClickEventData
        {
            EnvironmentName = environmentName,
            PitchClamp = _pitchClamp,
            YawClamp = _yawClamp,
            FollowSpeedX = _followSpeedX,
            FollowSpeedY = _followSpeedY,
            ObjectTransform = transform,
            Source = gameObject,
            Camera = this,
        };
    }
}
