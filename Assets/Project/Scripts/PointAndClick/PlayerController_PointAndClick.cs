using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController_PointAndClick : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CameraController PlayerCamera;

    [Header("Moving Settings")]
    [SerializeField] private float blinkAnimationSpeed = 0.5f;
    [SerializeField] private Animator blinkAnimator;

    [Header("Player Settings")]
    private Camera_Environment StartingPoint;
    [SerializeField] private Light flashLight;

    [Header("UI Settings")]
    [SerializeField] private float FadeDelay = 1f;

    [Header("Inventory")]
    [SerializeField] private EventClick_Item[] Inventory = new EventClick_Item[5];
    [SerializeField] private Inventory inventoryUI;

    [Header("Textboxes")]
    [SerializeField] private TextBox textBox;
    [SerializeField] private float thoughtTextDelay = 2.0f;

    [Header("GoalText")]
    [SerializeField] private TMP_Text GoalText;

    [Header("Scenes")]
    [SerializeField] private string fightingGameScene;
    [SerializeField] private string houseScene;
    [SerializeField] private string dreamScene;
    [SerializeField] private float transitionTimerInSeconds;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueStorage dialogueText;
    [SerializeField] private bool CanBeFastForwaded = false;

    [Header("Audio")]
    [SerializeField, Tooltip("Sound played when task is completed and player leaves PNC segment")] 
    private AudioSource finishTaskSource;
    [SerializeField, Tooltip("Clip played for above when morning task is finished")] 
    private AudioClip finishMorningClip;
    [SerializeField, Tooltip("Clip played for above when afternoon task is finished")] 
    private AudioClip finishAfternoonClip;

    private int dialogueIndex = 0;
    private bool startedDialogue = false;
    private int altDialogueIndex = 0;

    private Camera_Environment currentCamera;
    private Coroutine _hideInventoryCoroutine;
    private Coroutine _hideTextCoroutine;
    public TaskItem currentTask = null;

    private bool finishedFirstTeleport = false;
    private bool leftBedroom = false; //trigger a dialogue only after leaving the bedroom - HG
    private bool completeTask = false;

    //private enums for triggering 'day completed' and 'day started' sounds in AudioManager - HG
    private AudioTime dayFinished = AudioTime.Morning;
    private AudioTime dayStarted = AudioTime.Morning;
    private bool inventoryIsOpen = false;

    public static event System.Action<Boolean> OnTalking;

    private void OnEnable()
    {
        EventClick.OnObjectClicked += HandleObjectClicked;
        EventClick_GoalItem.GoalCompleted += HandleGoalCompleted;
        EventManager.ThoughtDialogue += HandleStartEvent;
    }

    private void OnDisable()
    {
        EventClick.OnObjectClicked -= HandleObjectClicked;
        EventClick_GoalItem.GoalCompleted -= HandleGoalCompleted;
        EventManager.ThoughtDialogue -= HandleStartEvent;
    }

    private void HandleStartEvent(DialogueStorage storage)
    {
        dialogueText = storage;
    }

    private void Awake()
    {
        PlayerCamera.rayCaster.enabled = false;
    }

    private void Start()
    {
        SetDayStartedAudio();
        string environment = PlayerPrefs.GetString("Environment");
        if (!string.IsNullOrEmpty(environment))
        {
            Camera_Environment[] allEnvironments = FindObjectsByType<Camera_Environment>(FindObjectsInactive.Include);
            foreach (Camera_Environment env in allEnvironments)
            {
                if (env.name == environment)
                {
                    StartingPoint = env;
                    break;
                }
            }
            if (StartingPoint == null)
            {
                StartingPoint = allEnvironments.FirstOrDefault();
            }          
        }
        else
        {
            StartingPoint = FindObjectsByType<Camera_Environment>(FindObjectsInactive.Include)[0];
        }
        if (StartingPoint != null)
        {
            StartingPoint.TeleportToSelf();
        }
        GoalText.text = PlayerPrefs.GetString("StartingTask");
    }

    //helper function for determining the correct audio clip to play when starting a new PNC seqeunce, based on time - HG
    private void SetDayStartedAudio()
    {
        TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
        int currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
        if (currentTimeOfDay == TimeOfDay.Morning)
        {
            dayStarted = AudioTime.Morning;
            Debug.Log("Set audio day start to morning!");
        }
        else if (currentTimeOfDay == TimeOfDay.Afternoon)
        {
            dayStarted = AudioTime.Afternoon;
            Debug.Log("Set audio day start to afternoon!");
        }
        else
        {
            dayStarted = AudioTime.Night;
            Debug.Log("Set audio day start to dream!");
        }
    }
    private void HandleObjectClicked(ClickEventData data)
    {

        if (finishedFirstTeleport) { AudioManager.instance.PlayInteractSound(); } //play interact audio on click  - HG

        switch(data)
        {
            case TeleportClickEventData point:
                MoveTo(point);
                break;
            case ItemClickEventData item:
                InteractWith(item);
                break;
            case NEIClickEventData nei:
                TalkAbout(nei);        
                break;
            case GoalEventData goal:
                InteractWith(goal);
                break;
            case FPBClickEventData fpb:
                EndDay(fpb);
                break;
            case TaskClickEventData task:
                StartCoroutine(StartTask(task));
                break;
        }
    }

    public void Transition(string level)
    {
        StartCoroutine(TransitionEnumerator(level));
    }

    private IEnumerator TransitionEnumerator(string level)
    {
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
        blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

        SceneManager.LoadScene(level);
    }

    // ********************************************************************************
    // Tasks
    // ********************************************************************************
    private IEnumerator StartTask(TaskClickEventData task)
    {
        if (task.DialogueText.useAltDialogue)
        {
            dialogueText = task.DialogueText;
            dialogueText.dialogue = new List<Talking>();
            dialogueText.dialogue.Add(dialogueText.alternativeDialogue[altDialogueIndex]);
            OpenDialogue();
            task.Giver.ChangeTaskStatus(false);

            yield return new WaitUntil(() => startedDialogue == false);

            altDialogueIndex = Mathf.Min(altDialogueIndex + 1, dialogueText.alternativeDialogue.Count - 1);
            task.Giver.ChangeTaskStatus(true);
        }
        else
        {
            currentTask = task.Task;    
            ChangingTask(task.Task.GoalText);
            dialogueText = task.DialogueText;
            OpenDialogue();
            task.Giver.ChangeTaskStatus(false);

            yield return new WaitUntil(() => startedDialogue == false);

            task.Giver.ChangeGoalStatus(true);
            OnOpenInventory();
            task.Giver.ChangeTaskStatus(true);
        }
    }

    private void ChangingTask(string newTask)
    {
        string strikethrough = "<s>" + GoalText.text + "</s>";
        GoalText.text = strikethrough + "\n";
        GoalText.text += newTask;
    }

    public void AddHint(string hintTask)
    {
        GoalText.text += "\n\t<i>" + hintTask + "</i>";
    }

    // ********************************************************************************
    // Final Point
    // ********************************************************************************

    public void ForceComplete()
    {
        completeTask = true;
    }

    private void EndDay(FPBClickEventData fpb)
    {
        if (completeTask)
        {
            EndingSequence(fpb.DialogueText);
        }
        else
        {
            dialogueText = new DialogueStorage();
            dialogueText.dialogue = fpb.DialogueText.alternativeDialogue;
            OpenDialogue();

            //textBox.SetText(fpb.IncompleteString);
            //OnShowTextBox();
        }
    }

    public void EndingSequence(DialogueStorage desc, float delay = 0)
    {
        string scene = "";
        TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
        int currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
        if (currentTimeOfDay == TimeOfDay.Morning)
        {
            scene = houseScene;
            PlayerPrefs.SetInt("TaskNum", currentTaskNum);
            PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Afternoon);
            Debug.Log("Set int to timeofday afternoon!");

            dayFinished = AudioTime.Morning; //set time to play correct audio clip on end sequence - HG
        }
        else if (currentTimeOfDay == TimeOfDay.Afternoon)
        {
            scene = fightingGameScene;
            PlayerPrefs.SetInt("TaskNum", currentTaskNum);
            PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Dream);
            Debug.Log("Set into to timeofday dream!");

            dayFinished = AudioTime.Afternoon; //set time to play correct audio clip on end sequence - HG
        }
        else
        {
            scene = dreamScene;
            PlayerPrefs.SetInt("TaskNum", currentTaskNum++);
            PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Morning);
            Debug.Log("Set int to timeofday morning!");

            dayFinished = AudioTime.Night; //set time to play correct audio clip on end sequence - HG
        }
        PlayerPrefs.Save();
        StartCoroutine(EndingSequence(desc, scene, delay));
    }

    public IEnumerator EndingSequence(DialogueStorage desc, string scene, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
            blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

        dialogueText = desc;
        OpenDialogue();

        //play corresponding audio - HG
        AudioManager.instance.PlayDayCompletedSound(dayFinished);

        yield return new WaitUntil(() => startedDialogue == false);

        SceneManager.LoadScene(scene);
    }

    private IEnumerator EndingSequence(string desc, string scene)
    {
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
            blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));
        

        textBox.SetText(desc);
        OnShowTextBox();

        //play corresponding audio - HG
        AudioManager.instance.PlayDayCompletedSound(dayFinished);

        yield return new WaitForSeconds(transitionTimerInSeconds);
        
        SceneManager.LoadScene(scene);
    }

    // ********************************************************************************
    // Camera Point
    // ********************************************************************************
    private void MoveTo(TeleportClickEventData data)
    {
        if (data.canEnter == false)
        {
            dialogueText = data.DialogueText;
            OpenDialogue();
            if (!data.alreadyClicked)
            {
                AddHint(data.requiredItemDesc);
            }
            OnOpenInventory();
            return;
        }
        StartCoroutine(TeleportSequence(data));
    }

    //teleport function for unique first blinking animation
    private IEnumerator TeleportSequenceFirst(TeleportClickEventData data)
    {
        if (currentCamera != null)
        {
            currentCamera.ActivateOrDeactivate(false);
        }

        transform.position = data.ObjectTransform.position;
        transform.rotation = data.ObjectTransform.rotation;


        yield return new WaitForSeconds(1.5f);
        bool isMorning = false;
        TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
        if (currentTimeOfDay == TimeOfDay.Morning)
        {
            isMorning = true;
        }

        AudioManager.instance.PlayDayStartedSound(dayStarted);
        yield return new WaitForSeconds(2.0f);

        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        yield return new WaitForSeconds(0.4f);
        blinkAnimator.SetTrigger("EyesStart");


        PlayerCamera.ChangeCameraSettings(data.PitchClamp, data.YawClamp, data.FollowSpeedX,
            data.FollowSpeedY, data.spin_360_x, data.spin_360_y);

        currentCamera = data.Camera;
        currentCamera.ActivateOrDeactivate(true);
    }

    private IEnumerator TeleportSequence(TeleportClickEventData data)
    {
        PlayerCamera.rayCaster.enabled = false;

        // Blinking
        if (!finishedFirstTeleport)
        {
            finishedFirstTeleport = true;

            yield return new WaitForSeconds(1.5f);
            bool isMorning = false;
            TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
            if (currentTimeOfDay == TimeOfDay.Morning)
            {
                isMorning = true;
            }

            AudioManager.instance.PlayDayStartedSound(dayStarted);
            yield return new WaitForSeconds(2.0f);

            blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
            yield return new WaitForSeconds(0.4f);
            blinkAnimator.SetTrigger("EyesStart");
        }
        else
        {
            blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
            blinkAnimator.SetTrigger("EyesDown");

            yield return new WaitUntil(() =>
            blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

            if (currentCamera != null)
            {
                currentCamera.ActivateOrDeactivate(false);
            }

            blinkAnimator.SetTrigger("EyesUp");
        }

        transform.position = data.ObjectTransform.position;
        transform.rotation = data.ObjectTransform.rotation;
        
        flashLight.enabled = data.ActivateFlashlight;

        PlayerCamera.ChangeCameraSettings(data.PitchClamp, data.YawClamp, data.FollowSpeedX, 
            data.FollowSpeedY, data.spin_360_x, data.spin_360_y);

        currentCamera = data.Camera;
        currentCamera.ActivateOrDeactivate(true);
        PlayerCamera.rayCaster.enabled = true;

        if (data.movementDialogue != null)
        {
            dialogueText = data.movementDialogue;
            OpenDialogue();
        }

        if (!leftBedroom)
        {
            leftBedroom = true;
            StartCoroutine(DisplayTaskStartDialogue());
        }

        if (data.endingCamera)
        {
            StartCoroutine(EndingSequence(data.endingDialogue, data.NextScene, data.delayBeforeEnding));
        }
    }

    // ********************************************************************************
    // Item
    // ********************************************************************************
    private void InteractWith(ItemClickEventData data)
    {
        AddToInventory(data.SourceItem);
    }

    private bool AddToInventory(EventClick_Item item)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null)
            {
                Inventory[i] = item;
                inventoryUI.AddItem(item.itemImage, item.itemName, i);
                textBox.SetName("Me");
                OnOpenInventory();
                dialogueText = item.dialogueText;
                OpenDialogue();
                return true;
            }
        }
        Debug.Log("Inventory is full");
        return false;
    }

    // ********************************************************************************
    // Goal
    // ********************************************************************************
    private void InteractWith(GoalEventData data)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] != null && data.SourceGoal.CollectItem(Inventory[i]))
            {
                Inventory[i] = null;
                inventoryUI.RemoveItem(i);
            }
        }
        data.SourceGoal.CheckGoal();
    }
    private void HandleGoalCompleted(GoalCompletionData data)
    {
        if (data.IsCompleted)
        {
            if (data.sequenceTask)
            {
                data.sequenceTask.IsUsedByGoal = false;
                data.sequenceTask.Activated = true;
                data.SourceGoal.enabled = false;
                AudioManager.instance.CallTaskCompletedSound();
            }
            else
            {
                if (data.Talked)
                {
                    dialogueText = data.compDialogueText;
                    OpenDialogue();
                    OnOpenInventory();
                }
                else
                {
                    dialogueText = data.DialogueText;
                    OpenDialogue();
                    ChangingTask(data.nextTask);
                    OnOpenInventory();
                    completeTask = true;
                    AudioManager.instance.CallTaskCompletedSound();
                }
            }
        }
        else
        {
            dialogueText = new DialogueStorage();
            dialogueText.dialogue = data.DialogueText.alternativeDialogue;
            OpenDialogue();
        }
    }

    // ********************************************************************************
    // Non Essential Item
    // ********************************************************************************
    private void TalkAbout(NEIClickEventData data)
    {
        dialogueText = data.DialogueText;
        OpenDialogue();
    }

    // ********************************************************************************
    // Textboxes (Auto)
    // ********************************************************************************
    public void OnShowTextBox()
    {
        if (textBox.IsEmpty())
            return;
        if (_hideTextCoroutine != null)
        {
            StopCoroutine(_hideTextCoroutine);
            _hideTextCoroutine = null;
        }

        textBox.HideTextBoxInstant(); // close instantly first
        textBox.ShowTextBox();        // then show the new text

        _hideTextCoroutine = StartCoroutine(HideTextAfterDelay(FadeDelay));
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        textBox.HideTextBox();
        _hideTextCoroutine = null;
    }

    // ********************************************************************************
    // Inventory
    // ********************************************************************************
    public void OnInventoryButtonPress(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (inventoryIsOpen)
        {
            if (_hideInventoryCoroutine != null)
                StopCoroutine(_hideInventoryCoroutine);

            inventoryUI.HideInventoryInstant();
            _hideInventoryCoroutine = null;
            inventoryIsOpen = false;
        }
        else
        {
            OnOpenInventory();
        }
    }

    public void OnOpenInventory()
    {
        inventoryIsOpen = true;
        inventoryUI.ShowInventory();

        if (_hideInventoryCoroutine != null)
        {
            StopCoroutine(_hideInventoryCoroutine);
        }
        _hideInventoryCoroutine = StartCoroutine(HideInventoryAfterDelay(FadeDelay));
    }

    private IEnumerator HideInventoryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        inventoryUI.HideInventory();
        _hideInventoryCoroutine = null;
        inventoryIsOpen = false;
    }

    // ********************************************************************************
    // Starting Thoughts
    // ********************************************************************************
    private IEnumerator DisplayTaskStartDialogue()
    {
        yield return new WaitForSeconds(thoughtTextDelay);
        OpenDialogue();
    }

    // ********************************************************************************
    // Dialogue Boxes
    // ********************************************************************************
    public void OpenDialogue()
    {
        dialogueIndex = 0;
        startedDialogue = true;
        PlayerCamera.rayCaster.enabled = false;
        GoThroughDialogue();
        OnTalking?.Invoke(true);
    }

    public void AdvanceDialogue(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!startedDialogue || !ctx.started) return;
        GoThroughDialogue();
    }

    private void GoThroughDialogue()
    {
        if (textBox.completeTextCrawl == false)
        {
            if (CanBeFastForwaded)
            {
                textBox.ShowTextBoxInstant();
                textBox.completeTextCrawl = true;
                return;
            }
            else
            {
                return;
            }
        }

        if (_hideTextCoroutine != null)
        {
            StopCoroutine(_hideTextCoroutine);
            _hideTextCoroutine = null;
        }

        if (dialogueIndex >= dialogueText.dialogue.Count)
        {
            dialogueIndex = 0;
            startedDialogue = false;
            textBox.HideTextBox();
            PlayerCamera.rayCaster.enabled = true;
            OnTalking?.Invoke(false);
        }
        else
        {
            textBox.SetText(dialogueText.dialogue[dialogueIndex].dialogue);
            textBox.SetName(dialogueText.dialogue[dialogueIndex].name);
            textBox.ShowTextBoxTextCrawl(dialogueText.dialogueSpeed, dialogueText.dialogue[dialogueIndex].sound);
            dialogueIndex++;
        }
    }
}
