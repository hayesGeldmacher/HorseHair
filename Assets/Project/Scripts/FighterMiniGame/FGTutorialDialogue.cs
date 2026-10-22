using System.Runtime.CompilerServices;
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
    [SerializeField] private Animator textBoxAnim; //animation for the text box to appear/disappear
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

        if (visible)
        {
            textBoxAnim.ResetTrigger("disappear");
            textBoxAnim.SetTrigger("appear");
        }
        else
        {
            textBoxAnim.ResetTrigger("appear");
            textBoxAnim.SetTrigger("disappear");
        }

        //dialoguePanel.alpha = visible ? 1f : 0f;
        //dialoguePanel.interactable = false;
        //dialoguePanel.blocksRaycasts = false;
    }
}