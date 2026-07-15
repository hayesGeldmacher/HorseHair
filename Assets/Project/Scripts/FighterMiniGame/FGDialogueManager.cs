using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class DialogueTrigger
{
    [TextArea]
    public string dialogueLine;
    public bool fromJesse = false;

    public bool hasBrotherResponse;
    public float brotherResponseTime = 1.0f;

    [TextArea]
    public string brotherDialogueLine;

    [Range(0f, 1f)]
    [Tooltip("Chance this dialogue triggers after the sequence matches 1 = always 0.5 = 50 percent 0 = never")]
    public float triggerChance = 1f;

    public float cooldown = 2f;
    public bool triggerOnlyOnce;

    [HideInInspector] public float cooldownTimer;
    [HideInInspector] public bool hasTriggered;
}

public class FGDialogueManager : MonoBehaviour
{
     #region Singleton

    public static FGDialogueManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of FG Dialogue Manager present in scene");
            return;
        }

        instance = this;
    }

    #endregion

    [System.Serializable]
    public class BrotherDialogue
    {
        public GameObject dialogueRoot;
        public TMP_Text dialogueText;
        public Animator dialogueAnim; //the animator which enables the text visibility - HG
        public float dialogueTimer;
        public bool isTalking = false;
        public DialogueSound soundType;
    }


    [Header("References")]
    public BrotherDialogue PlayerDialogue;
    public BrotherDialogue JesseDialogue;

    [Header("Customize")]
    [SerializeField] private float dialogueVisibleTime = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideDialogue(PlayerDialogue);
        HideDialogue(JesseDialogue);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDialogueTimer(PlayerDialogue);
        UpdateDialogueTimer(JesseDialogue);
    }


    public void TriggerDialogue(DialogueTrigger trigger)
    {

        //first, check if we should even trigger
        if (trigger.hasTriggered || trigger.triggerOnlyOnce) { return; }
    
        if (Random.value <= trigger.triggerChance)
        {
   
            //get who the dialogue is coming from
            BrotherDialogue targetDialogue = (trigger.fromJesse) ? JesseDialogue : PlayerDialogue;

            if (targetDialogue.dialogueText != null)
                targetDialogue.dialogueText.text = trigger.dialogueLine;

            AudioManager.instance.PlayDialogueBurst(trigger.dialogueLine, targetDialogue.soundType);

            ShowDialogue(targetDialogue);

            targetDialogue.dialogueTimer = dialogueVisibleTime;
            trigger.cooldownTimer = trigger.cooldown;
            trigger.hasTriggered = true;

            if (trigger.hasBrotherResponse)
            {
                BrotherDialogue responseDialogue = (trigger.fromJesse) ? PlayerDialogue : JesseDialogue;
                StartCoroutine(WaitForBrotherResponse(responseDialogue, trigger));
            }
        }
    }

    private IEnumerator WaitForBrotherResponse(BrotherDialogue responseDialogue, DialogueTrigger trigger)
    {
        yield return new WaitForSeconds(trigger.brotherResponseTime);
        if (!responseDialogue.isTalking)
        {
            TriggerResponse(responseDialogue, trigger.brotherDialogueLine);
        }
    }


    public void TriggerResponse(BrotherDialogue responseDialogue, string line)
    {
        responseDialogue.dialogueText.text = line;
        ShowDialogue(responseDialogue);
        AudioManager.instance.PlayDialogueBurst(line, responseDialogue.soundType);

        responseDialogue.dialogueTimer = dialogueVisibleTime;

    }

    private void ShowDialogue(BrotherDialogue dialogue)
    {
        //  if (dialogueRoot != null)
        //    dialogueRoot.SetActive(true);
        if (dialogue.dialogueAnim != null)
            dialogue.dialogueAnim.SetBool("visible", true);
        dialogue.isTalking = true;
    }

    private void HideDialogue(BrotherDialogue dialogue)
    {
        // if (dialogueRoot != null)
        //   dialogueRoot.SetActive(false);

        if (dialogue.dialogueAnim != null)
            dialogue.dialogueAnim.SetBool("visible", false);
        dialogue.isTalking = false;
    }

    private void UpdateDialogueTimer(BrotherDialogue dialogue)
    {
        if (dialogue.dialogueTimer <= 0f)
            return;

        dialogue.dialogueTimer -= Time.deltaTime;

        if (dialogue.dialogueTimer <= 0f)
            HideDialogue(dialogue);
    }


}
