using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;


public enum DialogueSound
{
    Player,
    Brother,
    Dad,
    Other,
}

public class AudioManager : MonoBehaviour
{
    #region Singleton

    public static AudioManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of playercontroller present in scene");
            return;
        }

        instance = this;
    }

    #endregion

    [Header("Interact Sound")]
    [SerializeField] private AudioSource interactSource; //sound when player selects an icon
    [SerializeField] private AudioSource hoverSource1; //sound1 when mouse hovers over an icon
    [SerializeField] private AudioSource hoverSource2; //sound 2 when mouse hovers over an icon
    private bool playedFirstHover = false;

    [Header("Start Task")]
    [SerializeField] private AudioSource startTaskSource; //sound when a new task PNC sequence begins
    [SerializeField] private AudioClip morningClip; //clip played for above sound when time of day is morning
    [SerializeField] private AudioClip afternoonClip; //clip played for above sound when time of day is afternoon

    [Header("Finish Task")]
    [SerializeField] private AudioSource taskCompletedSource; //source to be played at the time a task is completed

    [Header("Audio")]
    [SerializeField] private AudioSource finishTaskSource; //sound played when task is completed and player leaves PNC segment 
    [SerializeField] private AudioClip finishMorningClip; //clip played for above when morning task is finished
    [SerializeField] private AudioClip finishAfternoonClip; //clip played for above when afternoon task is finished

    [Header("Dialogue")]
    [SerializeField] private AudioSource[] dialogueSources;
    [SerializeField] private AudioClip[] playerClips; //index 0
    [SerializeField] private AudioClip[] brotherClips; //index 1
    [SerializeField] private AudioClip[] dadClips; //index 2

    [Header("Dialogue Burst")]
    [SerializeField] private float charsPerSound = 10; //how many sounds should trigger based on the characters in dialogue line
    [SerializeField] private float spaceBetweenSounds = 0.2f; //how long to wait between triggering dialogue sounds
    [SerializeField] private int maxSoundsPerLine = 4; //total sounds that can be triggered per line

    private int lastPlayedSource = 0;

    public void PlayInteractSound()
    {
        if(interactSource != null)
        {
            interactSource.pitch = GetRandomPitch();
            interactSource.Play();
        }
        else { Debug.LogWarning("No interact audio source slotted!"); }
    }

    public void PlayHoverSound()
    {
        //play two alternating audio effects so frequent hovers don't cut themselves off - HG
        AudioSource hoverSource = playedFirstHover? hoverSource2 : hoverSource1;
        playedFirstHover = !playedFirstHover;

        if(hoverSource != null)
        {
            hoverSource.pitch = GetRandomPitch();
            hoverSource.Play();
        }
        else { Debug.LogWarning("No hover audio source selected!"); }
    }

    public float GetRandomPitch()
    {
        return Random.Range(0.8f, 1.1f);
    }

    public void PlayStartTaskSound(bool isMorning)
    {
        if(startTaskSource == null) { Debug.LogWarning("No audio source slotted for start task sound!"); return; }
        if (isMorning && morningClip != null) { startTaskSource.clip = morningClip; }
        else if (!isMorning && afternoonClip != null) { startTaskSource.clip = afternoonClip; }

        startTaskSource.Play();
    }

    public void PlayTaskFinishSound(bool isMorning)
    {
        if(finishTaskSource == null) { Debug.LogWarning("No audio source slotted for finish task sound!"); return; }
        if(isMorning && finishMorningClip != false) { finishTaskSource.clip = finishMorningClip; }
        if (!isMorning && afternoonClip != null) { finishTaskSource.clip = finishAfternoonClip; }

        finishTaskSource.Play();
    }

    public void CallTaskCompletedSound()
    {
        StartCoroutine(PlayTaskCompletedSound());
    }

    private IEnumerator PlayTaskCompletedSound()
    {
        yield return new WaitForSeconds(0.5f);
       if (taskCompletedSource == null) { Debug.LogWarning("No audio source slotted for task completed sound!"); yield break; }
       taskCompletedSource.pitch = GetRandomPitch();
       taskCompletedSource.Play();
    }


    public void CallDialogueDelay()
    {
        StartCoroutine(PlayDialogueSound());
    }

    private IEnumerator PlayDialogueDelay()
    {
        yield return new WaitForSeconds(0.2f);
        PlayDialogueSound();
    }

    //plays a per-typed character sound, one at a time
    public void PlayDialogueSound(DialogueSound sound)
    {
        AudioClip clip = null;
        int audioIndex = 0;
        switch (sound)
        {
            case DialogueSound.Player:
                audioIndex = Random.Range(0, playerClips.Length);
                clip = playerClips[audioIndex];
                break;
            case DialogueSound.Brother:
                audioIndex = Random.Range(0, brotherClips.Length);
                clip = brotherClips[audioIndex];
                break;
            case DialogueSound.Dad:
                audioIndex = Random.Range(0, dadClips.Length);
                clip = dadClips[audioIndex];
                break;
            default:
                audioIndex = Random.Range(0, playerClips.Length);
                clip = playerClips[audioIndex];
                break;
        }

        AudioSource chosenSource = dialogueSources[0];
      
        int sourceIndex = 0;
        foreach (AudioSource source in dialogueSources)
        {
            if (lastPlayedSource != sourceIndex)
            {
                lastPlayedSource = sourceIndex;
                chosenSource = source;
                break;
            }

            sourceIndex++;
        }

        chosenSource.clip = clip;
        chosenSource.pitch = Random.Range(0.8f, 1.2f);
        chosenSource.Play();

        Debug.Log("Displayed Audio!");
    }

    //plays several dialogue sounds, spaced apart, based on length of string
    public void PlayDialogueBurst(string line, DialogueSound sound)
    {
        int charCount = 0;
        int soundCount = 0;
        float timeToWait = 0.0f;

        foreach(char c in line)
        {
            charCount++;
            if(charCount >= charsPerSound && soundCount <= maxSoundsPerLine)
            {
                charCount = 0;
                soundCount++;
                timeToWait += spaceBetweenSounds;
                StartCoroutine(WaitToPlayDialogue(sound, timeToWait));
            }
        }

        if(soundCount >= 0) { PlayDialogueSound(sound); }
    }

    private IEnumerator WaitToPlayDialogue(DialogueSound sound, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        PlayDialogueSound(sound);
    }
}
