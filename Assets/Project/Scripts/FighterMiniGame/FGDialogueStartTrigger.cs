using System.Collections;
using UnityEngine;

public class FGDialogueStartTrigger : MonoBehaviour
{
    /// <summary>
    /// This script manages dialogue during fighting games scenes that is NOT fighting-related
    /// ex. triggering dialogue at the start of the scene, during character selection, etc.
    /// </summary>
    /// 
    [Header("Start Dialogue")]
    [SerializeField] private bool hasStartDialogue;
    [SerializeField] private int startDialogueDelay;
    [SerializeField] private DialogueTrigger startTrigger;

    [Header("Character Select Dialogue")]
    [SerializeField] private bool hasCharSelectDialogue = false;
    [SerializeField] private int charSelectDelay;
    [SerializeField] private DialogueTrigger charSelectTrigger;
    private bool startedCharSelect = false;

    [Header("Dialogue References")]
    [SerializeField] private FGDialogueManager manager;
    [SerializeField] private FightRoundManager fightRoundManager;

    [Header("Audio Settings")]
    [SerializeField] private bool stopAudio = false;
    [SerializeField] private AudioSource stopSource;


    private void OnEnable()
    {
        if(hasCharSelectDialogue)
        fightRoundManager.onCharSelect += CharSelectStarted;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (hasStartDialogue) { StartCoroutine(DelayStartDialogue()); }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator DelayStartDialogue()
    {
        yield return new WaitForSeconds(startDialogueDelay);
        if(!startedCharSelect)
        manager.TriggerDialogue(startTrigger);
    }

    private IEnumerator DelayCharSelectDialogue()
    {
        yield return new WaitForSeconds(charSelectDelay);
        manager.TriggerDialogue(charSelectTrigger);
    }

    private void CharSelectStarted()
    {
        startedCharSelect = true;
        StartCoroutine(DelayCharSelectDialogue());

        if(stopAudio && stopSource != null)
        {
            stopSource.Stop();
        }
    }
}
