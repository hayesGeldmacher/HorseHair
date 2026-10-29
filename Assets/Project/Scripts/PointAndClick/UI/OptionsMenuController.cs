using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OptionsCategory
{
    General = 0,
    Audio = 1,
    Controls = 2
}

public class OptionsMenuController : MonoBehaviour
{
    [Header("Settings Panels")]
    [SerializeField] private CanvasGroup generalPanel;
    [SerializeField] private CanvasGroup audioPanel;
    [SerializeField] private CanvasGroup controlsPanel;

    [Header("Category Buttons")]
    [SerializeField] private Selectable generalButton;
    [SerializeField] private Selectable audioButton;
    [SerializeField] private Selectable controlsButton;

    [Header("First Settings")]
    [SerializeField] private Selectable generalFirstControl;
    [SerializeField] private Selectable audioFirstControl;
    [SerializeField] private Selectable controlsFirstControl;

    public bool IsEditingSettings { get; private set; }

    private OptionsCategory currentCategory = OptionsCategory.General;
    private Coroutine selectionCoroutine;

    public void OpenOptionsMenu()
    {
        currentCategory = OptionsCategory.General;
        IsEditingSettings = false;

        RefreshPanels();
        SelectNextFrame(generalButton);
    }

    public void PreviewCategory(OptionsCategory category)
    {
        if (IsEditingSettings)
            return;

        currentCategory = category;
        RefreshPanels();
    }

    public void CategoryPressed(OptionsCategory category)
    {
        if (IsEditingSettings && category == currentCategory)
        {
            ExitSettingsToCategory();
            return;
        }

        currentCategory = category;
        IsEditingSettings = true;

        RefreshPanels();
        SelectNextFrame(GetFirstControl(category));
    }

    public bool TryLeaveSettings()
    {
        if (!IsEditingSettings)
            return false;

        PlayerPrefs.Save();
        ExitSettingsToCategory();
        return true;
    }

    public void CloseOptionsMenu()
    {
        PlayerPrefs.Save();
        IsEditingSettings = false;
        RefreshPanels();
    }

    private void ExitSettingsToCategory()
    {
        IsEditingSettings = false;
        RefreshPanels();
        SelectNextFrame(GetCategoryButton(currentCategory));
    }

    private void RefreshPanels()
    {
        UpdatePanel(generalPanel, OptionsCategory.General);
        UpdatePanel(audioPanel, OptionsCategory.Audio);
        UpdatePanel(controlsPanel, OptionsCategory.Controls);
    }

    private void UpdatePanel(
        CanvasGroup panel,
        OptionsCategory category)
    {
        if (panel == null)
            return;

        bool visible = category == currentCategory;

        panel.gameObject.SetActive(visible);
        panel.alpha = 1f;
        panel.interactable = visible && IsEditingSettings;
        panel.blocksRaycasts = visible && IsEditingSettings;
    }

    private Selectable GetCategoryButton(OptionsCategory category)
    {
        switch (category)
        {
            case OptionsCategory.Audio:
                return audioButton;

            case OptionsCategory.Controls:
                return controlsButton;

            default:
                return generalButton;
        }
    }

    private Selectable GetFirstControl(OptionsCategory category)
    {
        switch (category)
        {
            case OptionsCategory.Audio:
                return audioFirstControl;

            case OptionsCategory.Controls:
                return controlsFirstControl;

            default:
                return generalFirstControl;
        }
    }

    private void SelectNextFrame(Selectable selectable)
    {
        if (selectionCoroutine != null)
            StopCoroutine(selectionCoroutine);

        selectionCoroutine = StartCoroutine(
            SelectAfterFrame(selectable));
    }

    private IEnumerator SelectAfterFrame(Selectable selectable)
    {
        yield return null;

        selectionCoroutine = null;

        if (selectable == null ||
            EventSystem.current == null ||
            !selectable.gameObject.activeInHierarchy)
        {
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(
        //    selectable.gameObject);
    }
}