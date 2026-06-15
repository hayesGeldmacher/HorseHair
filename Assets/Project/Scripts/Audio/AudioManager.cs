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
    [SerializeField] private AudioSource interactSource; //sound when player selects an icon
    [SerializeField] private AudioSource hoverSource1; //sound1 when mouse hovers over an icon
    [SerializeField] private AudioSource hoverSource2; //sound 2 when mouse hovers over an icon
    private bool playedFirstHover = false;



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
}
