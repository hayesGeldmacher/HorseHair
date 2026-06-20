using UnityEngine;
using UnityEngine.Audio;

public class AudioGroupFade : MonoBehaviour
{
    [Header("Background Audio")]
    [Tooltip("should background audio fade in from quiet")]
    [SerializeField] private bool fadeBackground = true; //should background audio start quiet and fade in?
    [SerializeField] private AudioMixer generalMixer;
    [SerializeField] private string backgroundParam = "backgroundVol"; //exposed audio parameter from the mixer

    [Range(5.0f, 25.0f)]
    [SerializeField] private float fadeSpeed = 5.0f;
    [Range(-80.0f, -10.0f)]
    [SerializeField] private float backgroundMin = -80.0f; //what's the lowest volume this mixer can go to
    [Range(-40.0f, 0.0f)]
    [SerializeField] private float backgroundMax = -35.0f; //waht's the highest volume this mixer can go to

    private bool backFadingIn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (fadeBackground) { StartBackgroundFade(); }
    }

    private void StartBackgroundFade()
    {
        generalMixer.SetFloat(backgroundParam, backgroundMin);
        backFadingIn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (backFadingIn) { BackgroundFadeUpdate(); }
    }

    private void BackgroundFadeUpdate()
    {
        float volume;
        generalMixer.GetFloat(backgroundParam, out volume);

        if (volume >= backgroundMax) { volume = backgroundMax; backFadingIn = false; }
        else
        {
            volume += fadeSpeed * Time.deltaTime;
        }

        generalMixer.SetFloat(backgroundParam, volume);
        //Debug.Log("CURRENT VOLUME: " + volume);
    }
}
