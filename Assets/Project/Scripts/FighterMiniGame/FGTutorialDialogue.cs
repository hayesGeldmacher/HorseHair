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

    private string keyboardMessage;
    private string controllerMessage;
    private bool hasInputVariants;
    private bool lastUsingController;

    public string CurrentMessage =>
        messageText != null ? messageText.text : "";

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
        hasInputVariants = false;

        if (dialoguePanel == null || messageText == null)
        {
            Debug.LogError(
                "Assign Dialogue Panel and Message Text.",
                this);
            return;
        }

        dialoguePanel.gameObject.SetActive(true);
        messageText.text = message;

        SetVisible(true);
    }

    public void Show(
        string keyboardText,
        string controllerText)
    {
        bool controller = ControlsGuideDisplay.UsingController;

        string controllerVersion =
            string.IsNullOrWhiteSpace(controllerText)
                ? keyboardText
                : controllerText;

        Show(controller ? controllerVersion : keyboardText);

        keyboardMessage = keyboardText;
        controllerMessage = controllerVersion;
        lastUsingController = controller;
        hasInputVariants = true;
    }

    private void Update()
    {
        if (!hasInputVariants || messageText == null)
            return;

        bool controller = ControlsGuideDisplay.UsingController;

        if (controller == lastUsingController)
            return;

        lastUsingController = controller;

        messageText.text = controller
            ? controllerMessage
            : keyboardMessage;
    }

    public void Hide()
    {
        hasInputVariants = false;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (dialoguePanel == null)
            return;

        dialoguePanel.interactable = false;
        dialoguePanel.blocksRaycasts = false;

        if (textBoxAnim != null)
        {
            textBoxAnim.ResetTrigger( visible ? "disappear" : "appear");

            textBoxAnim.SetTrigger( visible ? "appear" : "disappear");
        }
        else
        {
            dialoguePanel.alpha = visible ? 1f : 0f;
        }
    }
}