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
    private bool full360Camera_x = false;
    [SerializeField]
    private bool full360Camera_y = false;
    [SerializeField]
    private EventClick_Environment selfClickEvent;
    [SerializeField] private GameObject _arrows;
    [SerializeField] private GameObject _items;
    [SerializeField] private GameObject _cameraMesh;

    public TeleportClickEventData TeleportClickEventData;
    private EventClick_Environment[] connectedClickEvents;
    public EventClick[] connectedItems;

    private void OnValidate()
    {
        if (transform.parent != null)
        {
            // environmentName = "Camera_" + transform.parent.name;
            //  gameObject.name = environmentName;
            //commented out above lines so that in-game envrionment name is inspector-editable - HG
            gameObject.name = "Camera_" + transform.parent.name;
        }
    }

    private void Awake()
    {
        _cameraMesh.SetActive(false);
        connectedClickEvents = _arrows.GetComponentsInChildren<EventClick_Environment>();
        connectedItems = _items.GetComponentsInChildren<EventClick>();
    }

    private void Start()
    {
        if (TeleportClickEventData == null)
            SetUpEventData();

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
        foreach (var clickEvent in connectedItems)
        {
            clickEvent.ActivateOrDeactivate(State);
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
            spin_360_x = full360Camera_x,
            spin_360_y = full360Camera_y,
        };
    }
}
