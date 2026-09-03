using UnityEngine;

public class TravelScriptExtension_Base : MonoBehaviour
{
    public virtual void OnTravelToEnvironment(TeleportClickEventData eventData)
    {
        // This method can be overridden in derived classes to implement custom behavior when traveling to an environment.
        Debug.Log($"Traveling to environment: {eventData.EnvironmentName}");
    }
}
