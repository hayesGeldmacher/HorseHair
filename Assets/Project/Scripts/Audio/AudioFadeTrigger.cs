using UnityEngine;

public class AudioFadeTrigger : MonoBehaviour
{

    [Header("Pitch")]
    [SerializeField] private ClipPitchFade fade;
    [SerializeField] private float pitch;
    private bool triggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            fade.SetTargetPitch(pitch);
        }
    }
}
