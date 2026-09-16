using UnityEngine;

public class EndScene_AudioFade : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private AudioVolumeFade volumeFade;
    [SerializeField] private Transform playerBody; //the body of the camera

    [Header("Activation Fields")]
    [SerializeField] private float playerNearness;
    [SerializeField] private float nearThreshold;
    private bool activated = false;

    // Update is called once per frame
    void Update()
    {
        if (activated) { return; }
        if (playerBody != null)
        {
            playerNearness = Vector3.Distance(transform.position, playerBody.position);
            if (playerNearness <= nearThreshold) { CallVolumeFade(); }
        }
    }

    private void CallVolumeFade()
    {
        activated = true;
        volumeFade.StartFadeIn(false, false);
    }
}
