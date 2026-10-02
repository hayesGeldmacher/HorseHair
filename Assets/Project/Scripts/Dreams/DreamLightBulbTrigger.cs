using Unity.VisualScripting;
using UnityEngine;

public class DreamLightBulbTrigger : MonoBehaviour
{
    private bool triggered = false;
    [SerializeField] private DSCam cam;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered)
        {
            triggered = true;
            cam.DisableCameraFollow();
        }
    }
}
