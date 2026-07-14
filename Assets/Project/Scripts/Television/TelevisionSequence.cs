using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;

/// <summary>
/// This script is responsible for managing the television channel sequence that occurs each night
/// before starting the fighting game. Manages the start of the scene, changing channels, and transitioning to the fighting game.
/// </summary>

//struct for holding a single 'channel' for the television
[System.Serializable]
public struct Channel 
{
    public bool isTexture; //yes for animated texture channels, no for video player channels
    public RuntimeAnimatorController textureAnim; //only assign for texture channels
    public VideoClip video; //only assign for video channels
    public AudioClip audio; //audio to play for channel, assign for both texture and videos
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

    private int channelIndex = 0; //index for the current channel
    private int backupIndex = 1; //index for the next channel in sequence, for pre-loading videos

    [Header("Animation References")]
    [SerializeField] private Animator tvAnim; //animator for the channel TV screen
    [SerializeField] private Animator textureAnimController; //animator for the texture video canvas image
    [SerializeField] private Animator remoteAnim; //animator for the remote control model
    [SerializeField] private Animator controllerAnimPlayer; //animator for the player game controller model
    [SerializeField] private Animator controllerAnimBrother; //animator for the brother game controller model

    [Header("GameObject References")]
    [SerializeField] private GameObject televisionScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private FightRoundManager fightManager;

    [Header("Video References")]
    [SerializeField] private Video vp;
    [SerializeField] private Video cache;

    [Header("Audio References")]
    [SerializeField] private AudioSource tvAudio;

    [Header("Clicks Cooldown")]
    [SerializeField] private float clickCooldown = 1; //how long does player need to wait before clicking to change channel? 
    private float currentCooldown = 0;
    [SerializeField] private int totalClicks = 4; //how times does player change channels before ending tv sequence? 
    private int currentClicks = 0;

    [Header("Wait Times")]
    [Tooltip("how long for remote to leave.")]
    [SerializeField] private float remoteWait;
    [Tooltip("how long for brother controller to appear.")]
    [SerializeField] private float controllerWaitBrother;
    [Tooltip("how long for player controller to appear.")]
    [SerializeField] private float controllerWaitPlayer;


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
    }

    /// <summary>
    /// This is just for testing - will replace whole function with Ray interaction system hooks - HG
    /// </summary>
    void Update()
    {

        if(currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (canInteract)
            {
                if (!startedTelevision)
                {
                    startedTelevision = true;
                    StartTelevision();
                }
                else if(!calledChannelChange)
                {
                    if(currentClicks < totalClicks)
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
        yield return new WaitForSeconds(2.0f);
        remoteAnim.SetTrigger("equip");
        yield return new WaitForSeconds(1.0f);
        canInteract = true;
        //trigger some dialogue for the brother here
    }

    //when we integrate Ray's click commands, this will be played via that system -
    //for now, it simply operates based on clicking the mouse button
    public void StartTelevision()
    {
        tvAnim.SetTrigger("on");
        remoteAnim.SetTrigger("press");
        vp.player.Play();
        tvAudio.Play();
    }

    private IEnumerator ProgressChannels()
    {
        currentClicks++;
        calledChannelChange = true;
        currentCooldown = clickCooldown;
        remoteAnim.SetTrigger("press");
        yield return new WaitForSeconds(0.5f);
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
        if (newChannel.isTexture)
        {
            textureAnimController.runtimeAnimatorController = newChannel.textureAnim as RuntimeAnimatorController;
            cache.renderer.enabled = false;
            vp.renderer.enabled = false;
            
        }
        else
        { 

            Video vpClone = vp;
            Video cacheClone = cache;

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
            backupIndex = channelIndex + 1;
            if(backupIndex + 1 > channels.Length) {  backupIndex = 0; }

            //now, load the backup!
            cache.player.clip = channels[backupIndex].video;
            cache.player.Prepare();
        
        tvAudio.Stop();
        tvAudio.clip = newChannel.audio;
        tvAudio.Play();
        calledChannelChange = false;
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
        tvAnim.SetTrigger("off");
        yield return new WaitForSeconds(remoteWait);
        remoteAnim.SetTrigger("gone");

        yield return new WaitForSeconds(controllerWaitBrother);
        controllerAnimBrother.SetTrigger("equip");
        yield return new WaitForSeconds(controllerWaitPlayer);
        controllerAnimPlayer.SetTrigger("equip");
        televisionScreen.SetActive(false);
        gameScreen.SetActive(true);
        fightManager.SetGameActive();
    
        if(ScareManager.instance != null) { ScareManager.instance.CallScares(); }
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
}
