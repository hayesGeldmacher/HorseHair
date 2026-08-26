using UnityEngine;

[System.Serializable]
public class FadeGroup
{
    public AudioSource source;
    [Range(0.0f, 1.0f)]
    public float lowerLimit = 0.0f;
    [Range(0.0f, 1.0f)]
    public float upperLimit = 1.0f;
    public float speed = 0.05f;
}

public class AudioVolumeFade : MonoBehaviour
{
    public FadeGroup[] groups;
    [SerializeField] private bool currentlyFading = false;
    [SerializeField] private bool fadingUp = false;

    // Update is called once per frame
    void Update()
    {
        if (!currentlyFading) { return; }

        if (fadingUp)
        {
            bool finished = true;
            foreach(FadeGroup group in groups)
            {
                AudioSource source = group.source;
                if(source.volume < group.upperLimit)
                {
                    finished = false;
                    source.volume += group.speed * Time.deltaTime;
                }
                else
                {
                    source.volume = group.upperLimit;
                }
            }
            if (finished) { currentlyFading = false; }
        }
        else
        {
            bool finished = false;
            foreach (FadeGroup group in groups) 
            {
                AudioSource source = group.source;
                if (source.volume > group.lowerLimit)
                {
                    finished = false;
                    source.volume -= group.speed * Time.deltaTime;
                }
                else
                {
                    source.volume = group.lowerLimit;
                }
            }

            if (finished) { currentlyFading = false; }
        }
    }

    public void StartFadeIn(bool isFadingUp, bool play)
    {
        currentlyFading = true;
        fadingUp = isFadingUp;
        if (play)
        {
            foreach(FadeGroup group in groups)
            {
                AudioSource source = group.source;
                if (!source.isPlaying)
                {
                    source.Play();
                }
            }
        }
    }

    
}
