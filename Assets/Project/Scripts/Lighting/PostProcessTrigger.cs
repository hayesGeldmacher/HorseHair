using UnityEngine;

public class PostProcessTrigger : MonoBehaviour
{
    [Header("Vignette")]
    [SerializeField] private bool triggerVignette = true;
    [SerializeField] private float vignetteTarget = 1.0f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;

            if (triggerVignette) { PostProcessManager.instance.SetVignetteIntensity(vignetteTarget); }
        }
    }
}
