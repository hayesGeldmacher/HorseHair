using UnityEngine;
using UnityEngine.EventSystems;

public class btnFX : MonoBehaviour
{
    public AudioSource myFx;
    public AudioClip hoverFX;
    public AudioClip clickFX;

    public void HoverSound()
    {
        myFx.PlayOneShot(hoverFX);
    }

    public void ClickSound()
    {
        myFx.PlayOneShot(clickFX);
    }

    public void SelectSound(BaseEventData eventData)
    {
        if (eventData is AxisEventData)
        {
            HoverSound();
        }
    }
}