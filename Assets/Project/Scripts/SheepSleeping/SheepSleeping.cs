using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SheepSleeping : MonoBehaviour
{
    [SerializeField] private Animator sheepAnim;

    [SerializeField] private bool running; //is the sheep running? 

    [SerializeField] private float playerNearTrigger = 5.0f; //how close does player need to be for the trigger to activate? 
    [SerializeField] private bool hasTriggeredRun = false;
    [SerializeField] private Transform playerBody; //the body of the camera
    [SerializeField] private float playerNearness;
    [SerializeField] private float animTriggerWait = 2.0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTriggeredRun) { return; }
        if (playerBody != null)
        {
            playerNearness = Vector3.Distance(transform.position, playerBody.position);
            if (playerNearness <= playerNearTrigger) { CallParentRun(); }
        }
    }

    //triggers the sheep to run off into the distance
    public void CallParentRun()
    {
        if (!hasTriggeredRun)
        {
            hasTriggeredRun = true;
           StartCoroutine(TriggerStartAnim());
        }
    }

    private IEnumerator TriggerStartAnim()
    {
        yield return new WaitForSeconds(animTriggerWait);
        sheepAnim.SetBool("started", true);
    }

}
