using UnityEngine;

public class TelevisionSequence : MonoBehaviour
{
    /// <summary>
    /// This script manages the television sequence at the start of each night scene,
    /// before starting the fighting game
    /// </summary>


    [Header("Annimation")]
    [SerializeField] private Animator remoteAnim;

    private bool canInteract = false;

    []

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(0))
        {
            
        }
    }

    private IEnumerator BeginTelevisionScene()
    {
        yield return new WaitForSeconds(1.0f);
        remoteAnim.SetTrigger("equip");
        yield return new WaitForSeconds(1.0f);
        canInteract = true;
        //trigger some dialogue for the brother here
    }

    //when we integrate Ray's click commands, this will be played via that system -
    //for now, it simply operates based on clicking the mouse button
    public void StartTelevision()
    {

    }
}
