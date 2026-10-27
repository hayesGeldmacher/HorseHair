using System.Runtime.CompilerServices;
using UnityEngine;

public class DadStumbleAudio : MonoBehaviour
{
    [SerializeField] private bool shouldPlayAudio = false;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private float minWaitTime;
    [SerializeField] private float maxWaitTime;

    [SerializeField] private float currentWaitTime;

    [SerializeField] private AudioVolumeFade audioFade;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!shouldPlayAudio) return;

        if(currentWaitTime > 0)
        {
            currentWaitTime -= Time.deltaTime;
        }
        else
        {
            currentWaitTime = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
            PlayStumbleAudio();
        }

    }

    private void PlayStumbleAudio()
    {
       int randIndex = UnityEngine.Random.Range(0, clips.Length);
       AudioClip clip = clips[randIndex];
        source.clip = clip;
        source.Play();
    }

    public void ForceStop()
    {
        shouldPlayAudio = false;
        if(audioFade != null)
        {
         audioFade.StartFadeIn(false, false);
        }
        
    }

    
}
