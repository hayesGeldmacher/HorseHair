using UnityEngine;

public class AnimAudioCaller : MonoBehaviour
{

    [Header("Audio Settings")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.1f;
   public void CallRandomSound()
    {
        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];
        source.clip = clip;
        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();
    }
}
