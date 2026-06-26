using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float transitionTimerInSeconds;

    [Header("Audio")]
    [SerializeField] private AudioSource alarmSource;

    private Camera_Environment currentCamera;
    private Coroutine _hideInventoryCoroutine;
    private Coroutine _hideTextCoroutine;

    private bool finishedFirstTeleport = false;
    private bool leftBedroom = false; //trigger a dialogue only after leaving the bedroom - HG
    private bool completeTask = false;

    private void OnEnable()
    {
        EventClick.OnObjectClicked += HandleObjectClicked;
        EventClick_GoalItem.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        EventClick.OnObjectClicked -= HandleObjectClicked;
        EventClick_GoalItem.GoalCompleted -= HandleGoalCompleted;
    }

    private void Start()
    {
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
        }
        string goalText = PlayerPrefs.GetString("Goal");
        if (!string.IsNullOrEmpty(goalText))
        {
            GoalText.text = goalText;
        }
        if (StartingPoint != null)
        {
            StartingPoint.TeleportToSelf();
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
        }
    }

    private void EndDay(FPBClickEventData fpb)
    {
        if (completeTask)
        {
            string scene = "";
            TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);
            if (currentTimeOfDay == TimeOfDay.Morning)
            {
                scene = fightingGameScene;
            }
            else
            {
                scene = houseScene;
            }
            StartCoroutine(EndingSequence(fpb.CompleteString, scene));
        }
        else
        {
            textBox.SetText(fpb.IncompleteString);
            OnShowTextBox();
        }
    }

    private IEnumerator EndingSequence(string desc, string scene)
    {
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
            blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

        textBox.SetText(desc);
        OnShowTextBox();

        yield return new WaitForSeconds(transitionTimerInSeconds);

        SceneManager.LoadScene(scene);
    }

    private void MoveTo(TeleportClickEventData data)
    {
        //if this is the first teleport of the scene, play unique blinking animation - HG
        if (!finishedFirstTeleport)
        {
            StartCoroutine(TeleportSequenceFirst(data));
            finishedFirstTeleport = true;
        }
        else { StartCoroutine(TeleportSequence(data)); }
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
        if(alarmSource != null)
        {
            alarmSource.Play();
            yield return new WaitForSeconds(2.0f);
        }
        else { Debug.LogWarning("No alarm clock source slotted in Player!"); }

            blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        yield return new WaitForSeconds(0.4f);
        blinkAnimator.SetTrigger("EyesStart");


        PlayerCamera.ChangeCameraSettings(data.PitchClamp, data.YawClamp, data.FollowSpeedX,
            data.FollowSpeedY);

        currentCamera = data.Camera;
        currentCamera.ActivateOrDeactivate(true);
    }

    private IEnumerator TeleportSequence(TeleportClickEventData data)
    {
        if (currentCamera != null)
        {
            currentCamera.ActivateOrDeactivate(false);
        }
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
        blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

        transform.position = data.ObjectTransform.position;
        transform.rotation = data.ObjectTransform.rotation;

        PlayerCamera.ChangeCameraSettings(data.PitchClamp, data.YawClamp, data.FollowSpeedX, 
            data.FollowSpeedY);

        blinkAnimator.SetTrigger("EyesUp");
        currentCamera = data.Camera;
        currentCamera.ActivateOrDeactivate(true);

        if (!leftBedroom)
        {
            leftBedroom = true;
            StartCoroutine(DisplayTaskStartDialogue());
        }
    }

    private void InteractWith(ItemClickEventData data)
    {
        AddToInventory(data.SourceItem);
    }

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

    private bool AddToInventory(EventClick_Item item)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null)
            {
                Inventory[i] = item;
                inventoryUI.AddItem(item.itemImage, item.itemName, i);
                textBox.SetText(item.description);
                OnOpenInventory();
                OnShowTextBox();
                return true;
            }
        }
        Debug.Log("Inventory is full");
        return false;
    }

    private void TalkAbout(NEIClickEventData data)
    {
        textBox.SetText(data.Description);
        OnShowTextBox();
    }

    private void HandleGoalCompleted(GoalCompletionData data)
    {
        if (data.IsCompleted)
        {
            textBox.SetText(data.CompletedString);
            OnShowTextBox();
            GoalText.text = data.nextTask;
            completeTask = true;
        }
        else
        {
            textBox.SetText(data.NotCompletedString);
            OnShowTextBox();
        }
    }

    public void OnShowTextBox()
    {
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

    public void OnOpenInventory()
    {
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
    }

    private IEnumerator DisplayTaskStartDialogue()
    {
        yield return new WaitForSeconds(thoughtTextDelay);
        string text = PlayerPrefs.GetString("Thoughts");
        textBox.SetText(text);
        OnShowTextBox();
    }
}
