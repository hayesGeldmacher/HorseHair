using UnityEngine;
using UnityEngine.EventSystems;
public class EventClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pressed");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Released");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle pointer click event
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Handle pointer enter event
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Handle pointer exit event
    }
}