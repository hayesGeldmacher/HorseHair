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

    private void OnEnable()
    {
        EventClick.OnObjectClicked += HandleObjectClicked;
    }

    private void OnDisable()
    {
        EventClick.OnObjectClicked -= HandleObjectClicked;
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
        blinkAnimator.SetFloat("AnimationSpeed", blinkAnimationSpeed);
        blinkAnimator.SetTrigger("EyesDown");

        yield return new WaitUntil(() =>
        blinkAnimator.GetCurrentAnimatorStateInfo(0).IsName("EyesClosed"));

        transform.position = data.ObjectTransform.position;
        transform.rotation = data.ObjectTransform.rotation;

        PlayerCamera.ChangeCameraSettings(data.PitchClamp, data.YawClamp, data.FollowSpeed);

        blinkAnimator.SetTrigger("EyesUp");
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
