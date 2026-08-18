using System.Collections.Generic;
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
    [SerializeField] private bool _activateFlashlight = false;
    [Header("Ending Camera")]
    [SerializeField] private bool _endingCamera = false;
    [SerializeField] private float delayBeforeEnding = 2f;
    [SerializeField] private DialogueStorage endingDialogue;
    [SerializeField] private string NextScene;
    [Header("Environment Settings")]
    [SerializeField]
    private EventClick_Environment selfClickEvent;
    [SerializeField] private GameObject _arrows;
    [SerializeField] private GameObject _items;
    [SerializeField] private GameObject _cameraMesh;

    public TeleportClickEventData TeleportClickEventData;
    private EventClick_Environment[] connectedClickEvents;
    public EventClick[] connectedItems;
    private ItemDisappear[] connectedDisappearItems;

    [Header("Jumpscare Audio")]
    [SerializeField] private AudioSource jumpscareAudio;

    [Header("Tutorial")]
    [SerializeField] public DialogueMovement[] dialogueMovements;
    public bool talked = false;
    private Dictionary<(TimeOfDay, int), DialogueStorage> dialogueMovementDict = new Dictionary<(TimeOfDay, int), DialogueStorage>();

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
        connectedDisappearItems = _items.GetComponentsInChildren<ItemDisappear>();

        foreach (var dialogueMovement in dialogueMovements)
        {
            dialogueMovementDict.Add((dialogueMovement.time.timeOfDay, dialogueMovement.time.TaskNum), dialogueMovement.dialogue);
        }
    }

    private void Start()
    {
        if (TeleportClickEventData == null)
            SetUpEventData(null);

        ActivateOrDeactivate(false);
    }
    public void TeleportToSelf()
    {
        selfClickEvent.ForceClick();
        if(jumpscareAudio != null)
        {
            jumpscareAudio.Play();
        }
    }

    public void ActivateOrDeactivate(bool State)
    {
        gameObject.SetActive(State);  
        foreach (var clickEvent in connectedClickEvents)
        {
            clickEvent.ActivateOrDeactivate(State);
        }
        foreach (var clickEvent in connectedItems)
        {
            clickEvent.ActivateOrDeactivate(State);
        }
        foreach (var items in connectedDisappearItems)
        {
            items.ActivateOrDeactivate(State);
        }
    }

    public void SetUpEventData(EventClick_Environment _source)
    {
        TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
        int currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
        DialogueStorage used = (dialogueMovementDict.ContainsKey((currentTimeOfDay, currentTaskNum)) && !talked) ? 
            dialogueMovementDict[(currentTimeOfDay, currentTaskNum)] : null;
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
            source = _source,
            endingCamera = _endingCamera,
            delayBeforeEnding = delayBeforeEnding,
            endingDialogue = endingDialogue,
            NextScene = NextScene,
            ActivateFlashlight = _activateFlashlight,
            movementDialogue = used,           
        };
    }
}
