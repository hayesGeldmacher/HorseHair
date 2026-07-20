using UnityEngine;

public class DreamLightingTrigger : MonoBehaviour
{

    [Header("Directional Light")]
    [SerializeField] private bool triggerDirectional = true;
    [SerializeField] private float directionalTarget;

    [Header("Ambient Light")]
    [SerializeField] private bool triggerAmbience = true;
    [SerializeField] private float ambientTarget;

    [Header("Background Material")]
    [SerializeField] private Material mat;


    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;

            if (triggerDirectional) { DreamLightingManager.instance.SetDirectionalTarget(directionalTarget); }
            if (triggerAmbience) { DreamLightingManager.instance.SetAmbientTarget(ambientTarget); }
        }
    }

}
