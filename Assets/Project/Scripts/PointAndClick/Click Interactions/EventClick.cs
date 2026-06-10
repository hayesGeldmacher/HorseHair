using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEventData
{
    public Transform ObjectTransform;
    public GameObject Source;
}

public enum ObjectType
{
    None,
    Environment,
    Item,
    NEI
}

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int OutlineIndex = 1;

    protected ObjectType Type = ObjectType.None;
    public static event System.Action<ClickEventData> OnObjectClicked;
    public static event System.Action<ObjectType> OnObjectHovered;
    private Material outlineMaterial;

    private void Start()
    {
        outlineMaterial = GetComponent<Renderer>().materials[OutlineIndex];
        outlineMaterial.SetFloat("_Outline_Show", 0f);
        SetType();
    }

    protected virtual void SetType()
    {
        Type = ObjectType.None;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnObjectClicked?.Invoke(CreateEventData());
        outlineMaterial.SetFloat("_Outline_Show", 0f);
    }

    public void ForceClick()
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
        Debug.Log($"Pointer entered {gameObject.name}");
        outlineMaterial.SetFloat("_Outline_Show", 1f);
        OnObjectHovered?.Invoke(Type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Pointer exited {gameObject.name}");
        outlineMaterial.SetFloat("_Outline_Show", 0f);
        OnObjectHovered?.Invoke(ObjectType.None);
    }
}