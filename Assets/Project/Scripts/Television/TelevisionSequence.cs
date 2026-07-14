using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;

/// <summary>
/// This is not fucking fast enough - I think we will just need to create a few different video players that each render on or off...
/// </summary>

[System.Serializable]
public struct Channel 
{
    public bool isTexture;
    public RuntimeAnimatorController textureAnim;
    public VideoClip video;
    public AudioClip audio;
}

[System.Serializable]
public struct Video
{
    public VideoPlayer player;
    public MeshRenderer renderer;
}



public class TelevisionSequence : MonoBehaviour
{
    /// <summary>
    /// This script manages the television sequence at the start of each night scene,
    /// before starting the fighting game
    /// </summary>

    [SerializeField] private int channelIndex = 0;
    [SerializeField] private int backupIndex = 1;

    [Header("Animation")]
    [SerializeField] private Animator remoteAnim;
    [SerializeField] private Animator tvAnim;
    [SerializeField] private Animator controllerAnimPlayer;
    [SerializeField] private Animator controllerAnimBrother;


    [SerializeField] private Animator textureAnimController;

    private bool canInteract = false;
    private bool startedTelevision = false;

    [SerializeField] private GameObject televisionScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private FightRoundManager fightManager;

    [Header("Clicks Cooldown")]
    [SerializeField] private float clickCooldown = 1; //how long does player need to wait before clicking to change channel? 
    private float currentCooldown = 0;
    [SerializeField] private int totalClicks = 4;
    [SerializeField] private int currentClicks = 0;

    private bool calledChannelChange = false;
    [SerializeField] private bool calledEndSequence = false;
    [SerializeField] private bool skipTelevision = false;


    //basic structure for this:
    //2 video players 
    /// <summary>
    /// 1 player is the main, the other is the cache
    /// 
    /// when we are ready to swap:
    /// - let the cache load a new clip in its videoplayer
    /// when the cache is ready, simple disable the mesh renderer for the first cache and enable for teh second
    /// 
    /// as soon as they are swapped, we start prepping the video in the next cache!
    /// </summary>

    [Header("Video References")]
    [SerializeField] private Video vp;
    [SerializeField] private Video cache;

    [Header("Audio References")]
    [SerializeField] private AudioSource tvAudio;

    public Channel[] channels;

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

    // Update is called once per frame
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
        yield return new WaitForSeconds(0.5f);
        remoteAnim.SetTrigger("gone");

        yield return new WaitForSeconds(2.0f);
        controllerAnimBrother.SetTrigger("equip");
        yield return new WaitForSeconds(1.0f);
        controllerAnimPlayer.SetTrigger("equip");
        televisionScreen.SetActive(false);
        gameScreen.SetActive(true);
        fightManager.SetGameActive();
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
    }
}
