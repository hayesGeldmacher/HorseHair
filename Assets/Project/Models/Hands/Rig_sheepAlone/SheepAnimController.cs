using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SheepAnimController : MonoBehaviour
{

    [SerializeField] private Animator sheepParent; //ref to the owner entity, controls world movement
    [SerializeField] private Animator sheepChild; //ref to the animator for the actual rigged character

    [SerializeField] private bool running; //is the sheep running? 

    [SerializeField] private float playerNearTrigger = 5.0f; //how close does player need to be for the trigger to activate? 
    [SerializeField] private bool hasTriggeredRun = false;
    [SerializeField] private Transform playerBody; //the body of the camera
    [SerializeField] private float playerNearness;
    [SerializeField] private float runWait = 2.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       sheepChild.SetBool("running",  running);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTriggeredRun){ return; }
        if (playerBody != null ) 
        {
            playerNearness = Vector3.Distance(transform.position,  playerBody.position);
            if(playerNearness <= playerNearTrigger) { CallParentRun(); }
        }
    }

    //triggers the sheep to run off into the distance
    public void CallParentRun()
    {
        if (!hasTriggeredRun)
        {
            hasTriggeredRun = true;
            StartCoroutine(TriggerParentRun());
        }
    }

    private IEnumerator TriggerParentRun()
    {
        yield return new WaitForSeconds(runWait);
        sheepChild.SetBool("running", true);
        sheepParent.SetTrigger("run");
    }
}
