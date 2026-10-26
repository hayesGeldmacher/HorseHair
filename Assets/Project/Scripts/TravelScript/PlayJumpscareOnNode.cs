using UnityEngine;

public class NewMonoBehaviourScript : TravelScriptExtension_Base
{
    [SerializeField] private AudioSource jumpscareAudio;
    private bool played = false;


    public override void OnTravelToEnvironment(TeleportClickEventData eventData)
    {
        base.OnTravelToEnvironment(eventData);
        if (jumpscareAudio != null && !played) 
        {
            played = true;
            jumpscareAudio.Play();
        }
    }
}
