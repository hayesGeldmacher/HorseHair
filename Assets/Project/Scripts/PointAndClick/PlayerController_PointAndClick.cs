using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_PointAndClick : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CameraController PlayerCamera;


    [Header("Moving Settings")]
    [SerializeField] private float blinkAnimationSpeed = 0.5f;
    [SerializeField] private Animator blinkAnimator;

    [Header("Player Settings")]
    [SerializeField] private Camera_Environment StartingPoint;

    [Header("UI Settings")]
    [SerializeField] private float FadeDelay = 1f;

    [Header("Inventory")]
    [SerializeField] private EventClick_Item[] Inventory = new EventClick_Item[5];
    [SerializeField] private Inventory inventoryUI;

    [Header("Textboxes")]
    [SerializeField] private TextBox textBox;

    private Camera_Environment currentCamera;
    private Coroutine _hideInventoryCoroutine;
    private Coroutine _hideTextCoroutine;

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
        if (StartingPoint != null)
        {
            StartingPoint.TeleportToSelf();
        }
    }

    private void HandleObjectClicked(ClickEventData data)
    {
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
        }
    }

    private void MoveTo(TeleportClickEventData data)
    {
        StartCoroutine(TeleportSequence(data));
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
            Debug.Log("Goal Completed!");
        }
        else
        {
            string neededItems = "Items still needed:";
            foreach (var item in data.NeededItems)
            {
                if (!item.Value)
                {
                    neededItems = neededItems + " " + item.Key.itemName;
                }
            }
            Debug.Log(neededItems + ".");
        }
    }

    public void OnShowTextBox()
    {
        textBox.ShowTextBox();
        if (_hideTextCoroutine != null)
        {
            StopCoroutine(_hideTextCoroutine);
        }
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
}
