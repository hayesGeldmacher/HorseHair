using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

/// <summary>
/// This script is responsible for managing the television channel sequence that occurs each night
/// before starting the fighting game. Manages the start of the scene, changing channels, and transitioning to the fighting game.
/// </summary>


//struct for holding a single 'channel' for the television
[System.Serializable]
public struct Channel 
{
    public bool isTexture; //yes for animated texture channels, no for video player channels

    public bool UseCamera; //if enabled, use camera and render texture

    public Animator characterTrigger;// only assign for render cam channels

    public string animTriggerIdle;
    public string animTriggerCrawl; //only assing for render cam channels
    public float characterTriggerDelay; //delay to triggering above anim for cam channels
    public int position; //index for the list of positions to move horsecam to
    public Material material;
    public RuntimeAnimatorController textureAnim; //only assign for texture channels
    public VideoClip video; //only assign for video channels
    public AudioClip audio; //audio to play for channel, assign for both texture and videos

    public bool playOneOffSoundOnStart;
    public AudioSource oneOffSource;

    public bool hasDialogue;
    public DialogueTrigger trigger;
}

//struct for holding video player system
//two Videos in total - a 'main' player and a 'cache' player
//references swap back and forth to consistently pre-load videos before the channel switches
[System.Serializable]
public struct Video
{
    public VideoPlayer player; //video player component 
    public MeshRenderer renderer; //mesh renderer component, on same transform as above video player
}


public class TelevisionSequence : MonoBehaviour
{
    /// <summary>
    /// This script manages the television sequence at the start of each night scene,
    /// before starting the fighting game
    /// </summary>
    /// 
    [Header("Skip Sequence")]
    [Tooltip("Skip straight to fighting game.")]
    [SerializeField] private bool skipTelevision = false; //skip the television sequence, straight to fighting game
    [SerializeField] private bool dreamSequence = false;

    [Header("Next Scene")]
    [SerializeField] private string nextScene; //name of the next scene to load after television sequence

    [SerializeField] private int channelIndex = 0; //index for the current channel
    [SerializeField] private int backupIndex = 1; //index for the next channel in sequence, for pre-loading videos

    [Header("Animation References")]
    [SerializeField] private Animator tvAnim; //animator for the channel TV screen
    [SerializeField] private Animator reflectionAnim; //animator for the brother's face reflection in the screen
    [SerializeField] private Animator textureAnimController; //animator for the texture video canvas image
    [SerializeField] private Animator remoteAnim; //animator for the remote control model
    [SerializeField] private Animator controllerAnimPlayer; //animator for the player game controller model
    [SerializeField] private Animator controllerAnimBrother; //animator for the brother game controller model

    [Header("GameObject References")]
    [SerializeField] private GameObject televisionScreen;
    [SerializeField] private GameObject reflectionScreen;
    [SerializeField] private GameObject horseScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private FightRoundManager fightManager;

    [Header("Video References")]
    [SerializeField] private Video vp;
    [SerializeField] private Video cache;

    [Header("Audio References")]
    [SerializeField] private AudioSource tvAudio;
    [SerializeField] private AudioSource turnOffSource;

    [Header("Clicks Cooldown")]
    [SerializeField] private float clickCooldown = 1; //how long does player need to wait before clicking to change channel? 
    private float currentCooldown = 0;
    [SerializeField] private int totalClicks = 4; //how times does player change channels before ending tv sequence? 
    private int currentClicks = 0;

    [Header("Dialogue References")]
    [SerializeField] private DialogueTrigger endTrigger; //unique trigger for when the brothers transition to the fighting game

    [Header("Wait Times")]
    [Tooltip("how long for remote to leave.")]
    [SerializeField] private float remoteWait;
    [Tooltip("how long for brother controller to appear.")]
    [SerializeField] private float controllerWaitBrother;
    [Tooltip("how long for player controller to appear.")]
    [SerializeField] private float controllerWaitPlayer;

    [SerializeField] private PlayerController_PointAndClick playerControls;

    //the following is just for the final television sequence, other scenes dont have to touch
    [Header("Horse Cam")]
    [SerializeField] private Transform[] horseCamPositions;
    [SerializeField] private Transform horseCam;
    [SerializeField] private int horseCamElements = 3;
    private int horseCamElementCount = 0;
    [SerializeField] private AudioSource horseBreathAudio;
    [SerializeField] private AudioSource horseFootStepAudio;
    


    private bool calledChannelChange = false; //is the channel currently changing
    private bool calledEndSequence = false; //has the tv ending sequence been called yet
    private bool canInteract = false; //manages interact cooldown for changing channels
    private bool startedTelevision = false; //tracks if the tv has been enabled yet


    public Channel[] channels; //array of channels to swap between

    void Awake()
    {
        if (skipTelevision) { SkipTelevision(); }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!skipTelevision)
        {
            LoadBackupOnStart();
            StartCoroutine(BeginTelevisionScene());
        }
        if (dreamSequence)
        {
            if (!startedTelevision)
            {
                startedTelevision = true;
                StartTelevision();
            }
        }
    }

    public void InteractTelevision()
    {
        if (canInteract)
        {
            if (dreamSequence)
            {
                CallEndTelevisionSequence();
            }
            else
            {
                if (!startedTelevision)
                {
                    startedTelevision = true;
                    StartTelevision();
                }
                else if (!calledChannelChange)
                {
                    if (currentClicks < totalClicks)
                    {
                        StartCoroutine(ProgressChannels());
                    }
                    else
                    {
                        CallEndTelevisionSequence();
                    }
                }
            }
        }
    }

    private IEnumerator BeginTelevisionScene()
    {
        tvAnim.SetTrigger("on");
        yield return new WaitForSeconds(2.0f);
        remoteAnim.SetTrigger("equip");
        yield return new WaitForSeconds(1.0f);
        canInteract = true;
        StartTelevision();
    }

    public void StartTelevision()
    {
        remoteAnim.SetTrigger("press");
        if (!dreamSequence)
        {
            vp.player.Play();
            tvAudio.Play();

        }
        startedTelevision = true;

    }

    private IEnumerator ProgressChannels()
    {
        currentClicks++;
        calledChannelChange = true;
        currentCooldown = clickCooldown;
        remoteAnim.SetTrigger("press");
        yield return new WaitForSeconds(0.25f);
        channelIndex++;
        if(channelIndex + 1 > channels.Length) { channelIndex = 0; }

        LoadVideo();
    }

    private void LoadBackupOnStart()
    {
        vp.player.clip = channels[0].video;
        cache.player.clip = channels[1].video;
        cache.player.Prepare();
        tvAudio.clip = channels[0].audio;    
    }

    private void LoadVideo()
    {

        Channel newChannel = channels[channelIndex];

        if (newChannel.playOneOffSoundOnStart)
        {
            newChannel.oneOffSource.Play();
        }


        if (newChannel.isTexture)
        {
            if(horseScreen != null)
            {
                horseScreen.SetActive(false);
            }

            tvAnim.SetTrigger("blue");
            textureAnimController.runtimeAnimatorController = newChannel.textureAnim as RuntimeAnimatorController;
            cache.renderer.enabled = false;
            vp.renderer.enabled = false;

            
        }
        else if(!newChannel.UseCamera)
        {

            if (horseScreen != null)
            {
                horseScreen.SetActive(false);
            }

            Video vpClone = vp;
            Video cacheClone = cache;
            tvAnim.SetTrigger("blue");
            vp = cacheClone;
            cache = vpClone;

            //first disable the current video and set it to loading the next
            cache.renderer.enabled = false;
            cache.player.Stop();

            vp.renderer.enabled = true;
            if (vp.player.isPrepared)
            {
             vp.player.Play();
                Debug.Log("Player named: " + vp.player.gameObject.name + "is played next");

            }
            else
            {
                Debug.LogWarning("WARNING! player named: " + vp.player.gameObject.name + "is not prepared!");
            }
        }
        else
        {
            //using render cam channel

            horseCamElementCount++;
            
            //play footsteps when horse enters the door
            if(horseCamElementCount == horseCamElements)
            {
               horseBreathAudio.Play();
            }

            Debug.Log("Using Camera!");
            cache.renderer.enabled = false;
            vp.renderer.enabled = false;
            reflectionScreen.SetActive(false);
            gameScreen.SetActive(false);

            horseScreen.SetActive(true);
            tvAnim.ResetTrigger("blue");
            tvAnim.SetTrigger("red");
            MeshRenderer meshRender = horseScreen.GetComponent<MeshRenderer>();
            meshRender.material = newChannel.material;

            //move the horse to the new location
            horseCam.position = horseCamPositions[newChannel.position].position;
            horseCam.rotation = horseCamPositions[newChannel.position].rotation;

            //trigger associated animations
            StartCoroutine(TriggerChararcterAnim(newChannel));

        }

            backupIndex = channelIndex + 1;
            if(backupIndex + 1 > channels.Length) {  backupIndex = 0; }

            //now, load the backup!
            cache.player.clip = channels[backupIndex].video;
            cache.player.Prepare();
        
        tvAudio.Stop();
        tvAudio.clip = newChannel.audio;
        tvAudio.Play();
        calledChannelChange = false;

        if (newChannel.hasDialogue)
        {
            DialogueTrigger newTrigger = newChannel.trigger;
            StartCoroutine(CallDialogueTrigger(newTrigger));
        }
    }

    private IEnumerator CallDialogueTrigger(DialogueTrigger trigger)
    {
        yield return new WaitForSeconds(1.0f);
        FGDialogueManager.instance.TriggerDialogue(trigger);
    }

    private void CallEndTelevisionSequence()
    {
        if (!calledEndSequence)
        {
            calledEndSequence = true;
            StartCoroutine(EndTelevisionSequence());
        }
    }

    private IEnumerator EndTelevisionSequence()
    {
        tvAudio.Stop();
        if (!dreamSequence)
        {
            turnOffSource.Play();
        }
        tvAnim.SetTrigger("off");
        yield return new WaitForSeconds(remoteWait);
        remoteAnim.SetTrigger("gone");
        reflectionScreen.SetActive(true);
        televisionScreen.SetActive(false);
        reflectionAnim.SetTrigger("appear");
        yield return new WaitForSeconds(2.0f);
        if (!dreamSequence)
        {
            FGDialogueManager.instance.TriggerDialogue(endTrigger);
        }
        yield return new WaitForSeconds(6.0f);
        if (!dreamSequence) 
        {
            playerControls.Transition(nextScene);
        }
    }

    //testing function for skipping the television
    private void SkipTelevision()
    {
        tvAudio.Stop();
        controllerAnimBrother.SetTrigger("equip");
        controllerAnimPlayer.SetTrigger("equip");
        televisionScreen.SetActive(false);
        gameScreen.SetActive(true);
        fightManager.SetGameActive();
        remoteAnim.gameObject.SetActive(false);

        if (ScareManager.instance != null) { ScareManager.instance.CallScares(); }
    }


    //play associated animation for cam channels with anim properties
    private IEnumerator TriggerChararcterAnim(Channel channel)
    {
        if (channel.characterTrigger == null) yield break;
        channel.characterTrigger.SetTrigger(channel.animTriggerIdle);
        yield return new WaitForSeconds(channel.characterTriggerDelay);
        channel.characterTrigger.SetTrigger(channel.animTriggerCrawl);
    }

}
