using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEventData
{
    public Transform ObjectTransform;
    public GameObject Source;
}

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static event System.Action<ClickEventData> OnObjectClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnObjectClicked?.Invoke(CreateEventData());
    }

    protected virtual ClickEventData CreateEventData()
    {
        return new ClickEventData
        {
            ObjectTransform = transform,
            Source = gameObject
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}