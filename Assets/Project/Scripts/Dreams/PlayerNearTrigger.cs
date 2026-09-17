using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This script allows events to be triggered when the player is near the attached object
/// Intended to be used with child scripts that override base trigger
/// </summary>
public class PlayerNearTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform playerBody;

    [UnitHeaderInspectable("Nearness Fields")]
    [SerializeField] protected float currentNearness;
    [SerializeField] protected float nearnessThreshold;
    [SerializeField] protected bool activated;


    // Update is called once per frame
    virtual protected void Update()
    {
        if (activated) { return; }
        if (playerBody != null)
        {
            currentNearness = Vector3.Distance(transform.position, playerBody.position);
            if (currentNearness <= nearnessThreshold) { CallNearTrigger(); }
        }
    }
    
    virtual protected void CallNearTrigger()
    {
        activated = true;
        Debug.Log("Called Near Trigger!");
    }

}
