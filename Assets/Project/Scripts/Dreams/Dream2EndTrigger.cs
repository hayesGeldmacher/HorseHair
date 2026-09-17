using System.Collections;
using UnityEngine;

public class Dream2EndTrigger : PlayerNearTrigger
{

    [Header("References")]
    [SerializeField] private Animator horseWalkAnim;

    [Header("Animation Timing")]
    [SerializeField] private float triggerAnimDelay;

    [Header("Audio Timing")]
    [SerializeField] private AudioSource scareSource;
    [SerializeField] private float scareSourceDelay;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();   
    }

    protected override void CallNearTrigger()
    {
        base.CallNearTrigger();
        StartCoroutine(TriggerAnimation());
        StartCoroutine(PlayScareSound());
    }

    private IEnumerator TriggerAnimation()
    {
        yield return new WaitForSeconds(triggerAnimDelay);
        horseWalkAnim.SetTrigger("walk");
    }

    private IEnumerator PlayScareSound()
    {
        yield return new WaitForSeconds(scareSourceDelay);
        scareSource.Play();
    }
}
