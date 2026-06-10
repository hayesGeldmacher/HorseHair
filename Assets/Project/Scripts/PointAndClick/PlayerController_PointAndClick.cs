using System;
using System.Collections;
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

    private Camera_Environment currentCamera;

    private void OnEnable()
    {
        EventClick.OnObjectClicked += HandleObjectClicked;
    }

    private void OnDisable()
    {
        EventClick.OnObjectClicked -= HandleObjectClicked;
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
            case NPCClickEventData npc:   
                TalkTo(npc);
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
        Debug.Log(data.ItemName);
    }

    private void TalkTo(NPCClickEventData data)
    {
        Debug.Log(data.NPCName);
    }
}
