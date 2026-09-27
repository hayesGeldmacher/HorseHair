using UnityEngine;

public class FogTrigger : MonoBehaviour
{

    [SerializeField] private Transform playerObject;
    [SerializeField] private float playerNearness;
    [SerializeField] private float nearnessThreshold;

    [SerializeField] private float fogIntensity;
    [SerializeField] private bool fogTriggered;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fogTriggered) { return; }
        playerNearness = Vector3.Distance(playerObject.position, transform.position);
        if(playerNearness <= nearnessThreshold)
        {
            fogTriggered = true;
            TriggerFogIntensity();
        }
    }

    public void TriggerFogIntensity()
    {
        RenderSettings.fogDensity = fogIntensity;
    }
}
