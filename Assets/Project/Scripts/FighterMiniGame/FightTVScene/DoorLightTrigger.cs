using UnityEngine;
using System.Collections;

public class DoorLightTrigger : MonoBehaviour
{
    [Header("Door Trigger Times")]
    [SerializeField] private float triggerWaitMin;
    [SerializeField] private float triggerWaitMax;
    public float currentWait = 0;

    [SerializeField] private float doorOpenTimeMin = 1.0f;
    [SerializeField] private float doorOpenTimeMax = 4.0f;

    [Header("Door Trigger Limits")]
    [SerializeField] private float maxTriggers = 2;
    [SerializeField] private float currentTriggers = 0;

    [Header("Animation")]
    [SerializeField] private Animator doorLightAnim;

    [Header("Audio")]
    [SerializeField] private AudioSource doorSource;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;

    private float GetRandomWait()
    {
        return Random.Range(triggerWaitMin, triggerWaitMax);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWait = GetRandomWait();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentTriggers >= maxTriggers) { return; }
        if(currentWait <= 0)
        {
            currentWait = GetRandomWait();
            StartCoroutine(OpenDoor());
        }
        else
        {
            currentWait -= Time.deltaTime;
        }
    }

    private IEnumerator OpenDoor()
    {
        currentTriggers++;
        doorSource.clip = doorOpenClip;
        doorSource.pitch = Random.Range(0.9f, 1.1f);
        doorSource.Play();
        doorLightAnim.SetTrigger("open");

        float waitTime = Random.Range(doorOpenTimeMin, doorOpenTimeMax);
        yield return new WaitForSeconds(waitTime);

        CloseDoor();
    }

    private void CloseDoor()
    {
        doorSource.clip = doorCloseClip;
        doorSource.pitch = Random.Range(0.9f, 1.1f);
        doorSource.Play();
        doorLightAnim.SetTrigger("close");
    }
}
