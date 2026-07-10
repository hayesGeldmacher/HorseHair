using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Video;



[System.Serializable]
public struct Channel 
{
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

    [Header("Animation")]
    [SerializeField] private Animator remoteAnim;
    [SerializeField] private Animator tvAnim;

    private bool canInteract = false;
    private bool startedTelevision = false;

    [SerializeField] private GameObject televisionScreen;


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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BeginTelevisionScene());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canInteract)
            {
                if (!startedTelevision)
                {
                    startedTelevision = true;
                    StartTelevision();
                }
                else
                {
                    ProgressChannels();
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
    }

    public void ProgressChannels()
    {
        remoteAnim.SetTrigger("press");
        channelIndex++;
        if(channelIndex + 1 > channels.Length) { channelIndex = 0; }

        LoadVideo();
    }

    private void LoadVideo()
    {

        Video main = vp;
        Video backup = cache;
       
        vp = backup;
        cache = vp;

        main.renderer.enabled = true;
        backup.renderer.enabled = false;

        int backupIndex = (channelIndex + 1 > channels.Length) ? 0 : channelIndex + 1;

        //now, load the backup!
        backup.player.clip = channels[backupIndex].video;
        tvAudio.clip = channels[channelIndex].audio;
    }
}
