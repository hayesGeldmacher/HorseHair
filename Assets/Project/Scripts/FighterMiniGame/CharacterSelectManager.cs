using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a short, automatic character selection cut scene before the match begins
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPresentation
    {
        public string displayName;
        public Sprite portrait;

        [Tooltip("Visual model for fighter")]
        public GameObject fighterVisual;
    }

    [Header("References")]
    [SerializeField] private FightRoundManager roundManager;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Animator presentationAnimator;

    [Header("Player Presentation")]
    [SerializeField] private CharacterPresentation playerCharacter;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Opponent Presentation")]
    [SerializeField] private CharacterPresentation opponentCharacter;
    [SerializeField] private Image opponentPortraitImage;
    [SerializeField] private TMP_Text opponentNameText;

    [Header("Center Message")]
    [SerializeField] private TMP_Text centerMessageText;
    [SerializeField] private string readyMessage = "READY";

    [Header("Ready Rumble")]
    [Tooltip("Maximum distance in UI pixels that READY moves while rumbling")]
    [Min(0f)]
    [SerializeField] private float readyRumbleStrength = 10f;

    [Tooltip("Speed of the READY vibration")]
    [Min(0f)]
    [SerializeField] private float readyRumbleSpeed = 38f;

    [Tooltip("Maximum rotation in degrees during the READY rumble")]
    [Min(0f)]
    [SerializeField] private float readyRumbleRotation = 1.5f;

    [Tooltip("Scale punch applied when READY first appears")]
    [Range(0f, 0.5f)]
    [SerializeField] private float readyScalePunch = 0.12f;

    [Header("Timing")]
    [SerializeField] private float openingPause = 0.8f;
    [SerializeField] private float playerRevealTime = 1.8f;
    [SerializeField] private float opponentRevealTime = 1.8f;
    [SerializeField] private float readyMessageTime = 1.5f;

    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playerRevealSound;
    [SerializeField] private AudioClip opponentRevealSound;
    [SerializeField] private AudioClip readySound;

    private Coroutine presentationCoroutine;

    private Vector2 readyRestingPosition;
    private Quaternion readyRestingRotation;
    private Vector3 readyRestingScale;
    private bool readyTransformIsAnimating;

    private void Awake()
    {
        HideImmediately();
    }

    public void OpenSelection(float additionalOpeningDelay = 0f)
    {
        if (presentationCoroutine != null)
            StopCoroutine(presentationCoroutine);

        ResetReadyTransform();

        presentationCoroutine = StartCoroutine(PlayPresentation(additionalOpeningDelay));
    }

    public void HideAndReset()
    {
        if (presentationCoroutine != null)
        {
            StopCoroutine(presentationCoroutine);
            presentationCoroutine = null;
        }

        ResetReadyTransform();

        HideImmediately();
    }

    private IEnumerator PlayPresentation(float additionalOpeningDelay)
    {
        SetUpCharacters();

        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(true);

        SetPlayerPresentationVisible(true);
        SetOpponentPresentationVisible(true);

        playerNameText?.ForceMeshUpdate(true, true);
        opponentNameText?.ForceMeshUpdate(true, true);

        if (playerPortraitImage != null)
            playerPortraitImage.SetAllDirty();

        if (opponentPortraitImage != null)
            opponentPortraitImage.SetAllDirty();

        Canvas.ForceUpdateCanvases();

        SetPlayerPresentationVisible(false);
        SetOpponentPresentationVisible(false);
        SetCenterMessage("");

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(
            openingPause + additionalOpeningDelay
        );

        SetPlayerPresentationVisible(true);
        TriggerAnimation("ShowPlayer");
        PlaySound(playerRevealSound);

        yield return new WaitForSecondsRealtime(playerRevealTime);

        SetOpponentPresentationVisible(true);
        TriggerAnimation("ShowOpponent");
        PlaySound(opponentRevealSound);

        yield return new WaitForSecondsRealtime(opponentRevealTime);

        SetCenterMessage(readyMessage);
        TriggerAnimation("Ready");
        PlaySound(readySound);

        yield return AnimateReadyRumble(readyMessageTime);

        presentationCoroutine = null;
        roundManager?.StartMatchFromCharacterSelect();
    }

    private IEnumerator AnimateReadyRumble(float duration)
    {
        if (centerMessageText == null || duration <= 0f)
            yield break;

        RectTransform readyTransform = centerMessageText.rectTransform;

        readyRestingPosition = readyTransform.anchoredPosition;
        readyRestingRotation = readyTransform.localRotation;
        readyRestingScale = readyTransform.localScale;
        readyTransformIsAnimating = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float vibrationTime = elapsed * readyRumbleSpeed;
            float horizontalRumble =
                Mathf.Sin(vibrationTime) * 0.65f +
                Mathf.Sin(vibrationTime * 2.13f) * 0.35f;
            float verticalRumble =
                Mathf.Cos(vibrationTime * 1.37f) * 0.65f +
                Mathf.Sin(vibrationTime * 2.71f) * 0.35f;
            float rotationRumble = Mathf.Sin(vibrationTime * 1.73f);

            float fadeIn = Mathf.Clamp01(elapsed / 0.08f);
            float fadeOut = Mathf.Clamp01((duration - elapsed) / 0.15f);
            float rumbleAmount = fadeIn * fadeOut;

            float punchProgress = Mathf.Clamp01(elapsed / 0.18f);
            float scaleMultiplier = 1f +
                Mathf.Sin(punchProgress * Mathf.PI) * readyScalePunch;

            readyTransform.anchoredPosition = readyRestingPosition +
                new Vector2(horizontalRumble, verticalRumble) *
                readyRumbleStrength * rumbleAmount;

            readyTransform.localRotation = readyRestingRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotationRumble * readyRumbleRotation * rumbleAmount
                );

            readyTransform.localScale = readyRestingScale * scaleMultiplier;

            yield return null;
        }

        ResetReadyTransform();
    }

    private void ResetReadyTransform()
    {
        if (!readyTransformIsAnimating || centerMessageText == null)
            return;

        RectTransform readyTransform = centerMessageText.rectTransform;
        readyTransform.anchoredPosition = readyRestingPosition;
        readyTransform.localRotation = readyRestingRotation;
        readyTransform.localScale = readyRestingScale;
        readyTransformIsAnimating = false;
    }

    private void SetUpCharacters()
    {
        if (playerCharacter != null)
        {
            if (playerPortraitImage != null)
                playerPortraitImage.sprite = playerCharacter.portrait;

            if (playerNameText != null)
                playerNameText.text = playerCharacter.displayName;

            if (playerCharacter.fighterVisual != null)
                playerCharacter.fighterVisual.SetActive(true);
        }

        if (opponentCharacter != null)
        {
            if (opponentPortraitImage != null)
                opponentPortraitImage.sprite = opponentCharacter.portrait;

            if (opponentNameText != null)
                opponentNameText.text = opponentCharacter.displayName;

            if (opponentCharacter.fighterVisual != null)
                opponentCharacter.fighterVisual.SetActive(true);
        }
    }

    private void HideImmediately()
    {
        ResetReadyTransform();

        SetPlayerPresentationVisible(false);
        SetOpponentPresentationVisible(false);
        SetCenterMessage("");

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        if (characterSelectPanel != null)
            characterSelectPanel.SetActive(false);
    }

    private void SetPlayerPresentationVisible(bool isVisible)
    {
        if (playerPortraitImage != null)
            playerPortraitImage.gameObject.SetActive(isVisible);

        if (playerNameText != null)
            playerNameText.gameObject.SetActive(isVisible);
    }

    private void SetOpponentPresentationVisible(bool isVisible)
    {
        if (opponentPortraitImage != null)
            opponentPortraitImage.gameObject.SetActive(isVisible);

        if (opponentNameText != null)
            opponentNameText.gameObject.SetActive(isVisible);
    }

    private void SetCenterMessage(string message)
    {
        if (centerMessageText == null)
            return;

        centerMessageText.text = message;
        centerMessageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    private void TriggerAnimation(string triggerName)
    {
        if (presentationAnimator != null)
            presentationAnimator.SetTrigger(triggerName);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}