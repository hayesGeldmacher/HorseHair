using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEventData
{
    public Transform ObjectTransform;
    public GameObject Source;
    public string Description;
    public DialogueStorage DialogueText;
}

public enum ObjectType
{
    None,
    Environment,
    Item,
    NEI, // Non Essential Item
    Goal,
    AI, // Animated Item
    TI, // Transition Item
    FPB, // Final Point
    Talk,
    Task,
}

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int OutlineIndex = 1;
    [SerializeField] public string description;
    [SerializeField] private bool ResetAfterClick = true;
    [SerializeField] protected DialogueStorage dialogueText;
    [SerializeField] protected ObjectType resetCursor = ObjectType.None;

    protected ObjectType Type = ObjectType.None;
    protected string Name = "";
    public static event System.Action<ClickEventData> OnObjectClicked;
    public static event System.Action<ObjectType, string> OnObjectHovered;
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
        if (ResetAfterClick)
        {
            ResetClick();
        }
    }

    public virtual void ResetClick()
    {
        OnObjectHovered?.Invoke(resetCursor, "");
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
            Source = gameObject,
            Description = description,
            DialogueText = dialogueText,
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outlineMaterial.SetFloat("_Outline_Show", 1f);
        OnObjectHovered?.Invoke(Type, Name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outlineMaterial.SetFloat("_Outline_Show", 0f);
        OnObjectHovered?.Invoke(ObjectType.None, "");
    }

    public virtual void ActivateOrDeactivate(bool activate)
    {
        gameObject.SetActive(activate);
    }
}