using Unity.VisualScripting;
using UnityEngine;

public class AudioFadeCall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TransitionData_Call call;

    [SerializeField] private AudioVolumeFade fade; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        call.OnTransitionCalled += CalledTransition;
    }

    public void CalledTransition()
    {
        Debug.Log("Started Fading out!");
        fade.StartFadeIn(false, false);
    }
}
