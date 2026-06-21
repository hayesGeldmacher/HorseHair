using UnityEngine;




public class ClipPitchFade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource source;

    [Header("Pitch")]
    [SerializeField] private float pitchChangeSpeed = 1.0f;
    private float currentPitch;
    private float targetPitch;
    private float previousPitch;
    private bool hasPitchTarget = false;
    private float pitchLerpTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPitch = source.pitch;
        previousPitch = currentPitch;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPitchTarget) { PitchUpdate(); }
    }

    private void PitchUpdate()
    {
        currentPitch = Mathf.Lerp(previousPitch,  currentPitch, pitchLerpTime);
        pitchLerpTime += pitchChangeSpeed * Time.deltaTime * 0.6f;
        if(pitchLerpTime >= 1.0f)
        {
            hasPitchTarget = false;
            pitchLerpTime = 0.0f;
        }

        source.pitch = currentPitch;
    }

    public void SetTargetPitch(float volume)
    {
        previousPitch = currentPitch;
        targetPitch = volume;
        hasPitchTarget = true;
        pitchLerpTime = 0;
    }
}
