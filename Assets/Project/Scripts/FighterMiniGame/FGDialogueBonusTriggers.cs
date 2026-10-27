using UnityEngine;
using System.Collections;

public class FGDialogueBonusTriggers : MonoBehaviour
{
    [Header("Occurs during the horse fight")]
    public DialogueTrigger midFightTrigger0;

    public DialogueTrigger midFightTrigger1;

    public DialogueTrigger postFightTrigger0;
    public DialogueTrigger postFightTrigger1;
    public float postFightFinalDelay = 5.0f;


    [SerializeField] private FGDialogueManager manager;

    public void CallFightTrigger(int index, float delay)
    {
        StartCoroutine(DelayFightTrigger(index, delay));
    }

    private IEnumerator DelayFightTrigger(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        DialogueTrigger trigger = midFightTrigger0;
        if(index == 1)
        {
            trigger = midFightTrigger1;
        }
        else if(index == 2)
        {
            trigger = postFightTrigger0;
        }

        manager.TriggerDialogue(trigger);
    }

    public void CallFinalFightDialogue()
    {
        StartCoroutine(DelayFinalDialogueTrigger());
    }
    private IEnumerator DelayFinalDialogueTrigger()
    {
        yield return new WaitForSeconds(postFightFinalDelay);
        manager.TriggerDialogue(postFightTrigger1);
    }
}
