using System;
using System.Collections;
using UnityEngine;

public class EventClick_End : MonoBehaviour
{
    [SerializeField] private EventClick_Environment TiedEnd;
    [SerializeField] private PlayerController_PointAndClick PNC;
    [SerializeField] private DialogueStorage endingDialogue;
    [SerializeField] private string NextScene;
    [SerializeField] private float endDelay; //if over 0, delays ending the scene for x seconds - HG

    void OnEnable()
    {
        EventClick_Environment.OnObjectClicked += HandleObjectClicked;
    }

    void OnDisable()
    {
        EventClick_Environment.OnObjectClicked -= HandleObjectClicked;
    }

    private void Start()
    {
        TiedEnd.EndingCamera = true;
    }

    private void HandleObjectClicked(ClickEventData data)
    {
        if (data is TeleportClickEventData teleportData && teleportData.source == TiedEnd)
        {
            //StartCoroutine(DelayEnd());
            //Make sure time of day is set correctly
            PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Morning);
            // Use 0 for testing if day 3 isn't set yet, 2 otherwise
            PlayerPrefs.SetInt("TaskNum", 2);
            PlayerPrefs.Save();

            PNC.ForceComplete();
            StartCoroutine(PNC.EndingSequence(endingDialogue, NextScene));
        }
    }

    private IEnumerator DelayEnd()
    {
        yield return new WaitForSeconds(endDelay);
        //Make sure time of day is set correctly
        PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Morning);
        // Use 0 for testing if day 3 isn't set yet, 2 otherwise
        PlayerPrefs.SetInt("TaskNum", 2);
        PlayerPrefs.Save();

        PNC.ForceComplete();
        StartCoroutine(PNC.EndingSequence(endingDialogue, NextScene));

    }
}
