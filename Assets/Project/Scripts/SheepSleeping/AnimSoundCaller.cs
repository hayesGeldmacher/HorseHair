using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public struct AudioPair
{
    public AudioClip clip;
    public AudioSource source;
}

public class AnimSoundCaller : MonoBehaviour
{

    [Header("Audio Slots")]
    [SerializeField] private AudioPair[] audioSlots;

    [Header("Audio Settings")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    
    public void PlayAudioSlot(int index)
    {
       if(index > audioSlots.Length)
        {
            Debug.LogWarning("tried to call index out of range in AnimSoundCaller!");
            return;
        }

       AudioPair slot = audioSlots[index];
       AudioSource source = slot.source;
       AudioClip clip = slot.clip;

       if(clip == null || source == null) 
        {
            Debug.LogWarning("tried to call audio pair with null!");
            return; 
       }

        source.pitch = Random.Range(minPitch, maxPitch);
        source.clip = clip;
        source.Play();
    }
}
