using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OptionsCategoryButton :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerDownHandler,
    ISelectHandler,
    ISubmitHandler
{
    [SerializeField] private OptionsMenuController optionsMenu;
    [SerializeField] private OptionsCategory category;

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (optionsMenu == null)
            return;

        if (optionsMenu.IsEditingSettings)
            return;

        optionsMenu.PreviewCategory(category);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                gameObject
            );
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (optionsMenu == null)
            return;

        if (!optionsMenu.IsEditingSettings)
            optionsMenu.PreviewCategory(category);
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        PressCategory();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PressCategory();
    }

    private void PressCategory()
    {
        if (optionsMenu == null)
        {
            Debug.LogError(
                $"{name}: OptionsMenuController is not assigned.",
                this
            );

            return;
        }

        optionsMenu.CategoryPressed(category);
    }
}