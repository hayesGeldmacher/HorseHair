using TMPro;
using UnityEngine;

/// <summary>
/// This is used to show tutorial messages to the player during the game.
/// </summary>
public class FGTutorialDialogue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup dialoguePanel;
    [SerializeField] private TMP_Text messageText;

    private void Reset()
    {
        dialoguePanel = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (dialoguePanel == null)
            dialoguePanel = GetComponent<CanvasGroup>();
    }

    public void Show(string message)
    {
        if (dialoguePanel == null)
        {
            Debug.LogError(
                "FGTutorialDialogue needs a CanvasGroup assigned to Dialogue Panel.",
                this);
            return;
        }

        if (messageText == null)
        {
            Debug.LogError(
                "FGTutorialDialogue needs a TMP text assigned to Message Text.",
                this);
            return;
        }

        dialoguePanel.gameObject.SetActive(true);
        messageText.text = message;
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (dialoguePanel == null)
            return;

        dialoguePanel.alpha = visible ? 1f : 0f;
        dialoguePanel.interactable = false;
        dialoguePanel.blocksRaycasts = false;
    }
}