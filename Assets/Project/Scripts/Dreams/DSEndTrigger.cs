using UnityEngine;

public class DSEndTrigger : MonoBehaviour
{

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            DSManager.instance.CallEndScene();
        }
    }
}
