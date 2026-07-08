using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OptionsCategory
{
    Display,
    Audio
}

public class OptionsMenuController : MonoBehaviour
{
    [Header("Settings Panels")]
    [SerializeField] private CanvasGroup displayPanel;
    [SerializeField] private CanvasGroup audioPanel;

    [Header("Category Buttons")]
    [SerializeField] private Selectable displayButton;
    [SerializeField] private Selectable audioButton;

    [Header("First Settings")]
    [SerializeField] private Selectable displayFirstControl;
    [SerializeField] private Selectable audioFirstControl;

    public bool IsEditingSettings { get; private set; }

    private OptionsCategory currentCategory = OptionsCategory.Display;
    private Coroutine selectionCoroutine;

    public void OpenOptionsMenu()
    {
        currentCategory = OptionsCategory.Display;
        IsEditingSettings = false;

        ShowCategory(currentCategory);
        DisableSettingsInteraction();
        SelectNextFrame(displayButton);
    }

    public void PreviewCategory(OptionsCategory category)
    {
        if (IsEditingSettings)
            return;

        currentCategory = category;

        ShowCategory(category);
        DisableSettingsInteraction();
    }

    public void CategoryPressed(OptionsCategory category)
    {
        if (IsEditingSettings)
        {
            if (category == currentCategory)
            {
                ExitSettingsToCategory(category);
                return;
            }

            EnterSettings(category);
            return;
        }

        EnterSettings(category);
    }

    private void EnterSettings(OptionsCategory category)
    {
        currentCategory = category;
        IsEditingSettings = true;

        ShowCategory(category);

        bool isDisplay =
            category == OptionsCategory.Display;

        SetPanelInteraction(displayPanel, isDisplay);
        SetPanelInteraction(audioPanel, !isDisplay);

        Selectable firstControl = isDisplay
            ? displayFirstControl
            : audioFirstControl;

        SelectNextFrame(firstControl);
    }

    private void ExitSettingsToCategory(
        OptionsCategory category)
    {
        currentCategory = category;
        IsEditingSettings = false;

        ShowCategory(category);
        DisableSettingsInteraction();

        Selectable categoryButton =
            category == OptionsCategory.Display
                ? displayButton
                : audioButton;

        SelectNextFrame(categoryButton);
    }

    public bool TryLeaveSettings()
    {
        if (!IsEditingSettings)
            return false;

        ExitSettingsToCategory(currentCategory);
        return true;
    }

    public void CloseOptionsMenu()
    {
        IsEditingSettings = false;
        DisableSettingsInteraction();
    }

    private void ShowCategory(OptionsCategory category)
    {
        if (displayPanel != null)
        {
            displayPanel.gameObject.SetActive(
                category == OptionsCategory.Display
            );
        }

        if (audioPanel != null)
        {
            audioPanel.gameObject.SetActive(
                category == OptionsCategory.Audio
            );
        }
    }

    private void DisableSettingsInteraction()
    {
        SetPanelInteraction(displayPanel, false);
        SetPanelInteraction(audioPanel, false);
    }

    private static void SetPanelInteraction(
        CanvasGroup panel,
        bool canInteract)
    {
        if (panel == null)
            return;

        panel.alpha = 1f;
        panel.interactable = canInteract;
        panel.blocksRaycasts = canInteract;
    }

    private void SelectNextFrame(Selectable selectable)
    {
        if (selectionCoroutine != null)
            StopCoroutine(selectionCoroutine);

        selectionCoroutine =
            StartCoroutine(SelectAfterFrame(selectable));
    }

    private IEnumerator SelectAfterFrame(
        Selectable selectable)
    {
        yield return null;

        if (selectable == null ||
            EventSystem.current == null ||
            !selectable.gameObject.activeInHierarchy)
        {
            selectionCoroutine = null;
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            selectable.gameObject
        );

        selectionCoroutine = null;
    }
}