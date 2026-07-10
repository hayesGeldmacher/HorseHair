using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TelevisionSequence : MonoBehaviour
{
    /// <summary>
    /// This script manages the television sequence at the start of each night scene,
    /// before starting the fighting game
    /// </summary>


    [Header("Annimation")]
    [SerializeField] private Animator remoteAnim;
    [SerializeField] private Animator tvAnim;

    private bool canInteract = false;
    private bool startedTelevision = false;

    [SerializeField] private GameObject televisionScreen;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BeginTelevisionScene());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canInteract)
            {
                if (!startedTelevision)
                {
                    startedTelevision = true;
                    StartTelevision();
                }
            }
        }
    }

    private IEnumerator BeginTelevisionScene()
    {
        yield return new WaitForSeconds(2.0f);
        remoteAnim.SetTrigger("equip");
        yield return new WaitForSeconds(1.0f);
        canInteract = true;
        //trigger some dialogue for the brother here
    }

    //when we integrate Ray's click commands, this will be played via that system -
    //for now, it simply operates based on clicking the mouse button
    public void StartTelevision()
    {
        tvAnim.SetTrigger("on");
        remoteAnim.SetTrigger("press");
    }

    public void ProgressChannels()
    {
        remoteAnim.SetTrigger("press");
    }
}
