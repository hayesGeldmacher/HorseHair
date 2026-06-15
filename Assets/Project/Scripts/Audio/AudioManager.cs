using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private AudioSource interactSource;


    public void PlayInteractSound()
    {
        if(interactSource != null)
        {
            interactSource.pitch = GetRandomPitch();
            interactSource.Play();
        }
        else { Debug.LogWarning("No interact audio source slotted!"); }
    }

    public float GetRandomPitch()
    {
        return Random.Range(0.8f, 1.1f);
    }
}
