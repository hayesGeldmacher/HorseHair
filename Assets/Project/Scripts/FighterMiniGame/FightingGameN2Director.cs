using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FightingGameN2Director : MonoBehaviour
{
    [Header("Fight")]
    [SerializeField] private FightRoundManager roundManager;
    [SerializeField] private FightCharacter farmer;
    [SerializeField] private FightCharacter sheep;
    [SerializeField] private FightCharacter horse;

    [Header("Bonus Transition")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private float normalWinScreenDuration = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource dadArgumentSource;
    [SerializeField] private AudioSource fightMusicSource;
    [SerializeField] private AudioClip bonusFightMusic;

    [Header("Horse Drag Sequence")]
    [SerializeField] private Transform farmerDragRoot;
    [SerializeField] private Animator farmerAnimator;
    [SerializeField] private Animator horseAnimator;
    [SerializeField] private Transform dragEndPoint;
    [SerializeField] private float dragStartDelay = 2f;
    [SerializeField] private float dragDuration = 5f;
    [SerializeField] private string horseDragTrigger = "drag";

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    private bool bonusMatchActive;
    private bool runningSequence;

    private void Start()
    {
        if (horse != null)
            horse.gameObject.SetActive(false);

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }

        if (dadArgumentSource != null)
            dadArgumentSource.Play();
    }

    private void OnEnable()
    {
        if (roundManager != null)
            roundManager.MatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        if (roundManager != null)
            roundManager.MatchEnded -= HandleMatchEnded;
    }

    private void HandleMatchEnded(FightCharacter winner)
    {
        if (runningSequence)
            return;

        runningSequence = true;

        if (bonusMatchActive)
            StartCoroutine(PlayFinale());
        else
            StartCoroutine(BeginBonusRound());
    }

    private IEnumerator BeginBonusRound()
    {
        // Leave the normal victory screen visible briefly.
        yield return new WaitForSecondsRealtime(
            normalWinScreenDuration
        );

        // Cover the character swap.
        yield return FadeTo(1f);

        // Disable the complete sheep gameplay object.
        if (sheep != null)
            sheep.gameObject.SetActive(false);

        // Enable the complete horse gameplay object.
        if (horse != null)
        {
            horse.gameObject.SetActive(true);

            Rigidbody horseBody =
                horse.GetComponent<Rigidbody>();

            if (horseBody != null)
            {
                horseBody.linearVelocity = Vector3.zero;
                horseBody.angularVelocity = Vector3.zero;
            }

            if (horseAnimator != null)
            {
                horseAnimator.speed = 1f;
                horseAnimator.applyRootMotion = false;

                if (!string.IsNullOrWhiteSpace(
                    horseDragTrigger
                ))
                {
                    horseAnimator.ResetTrigger(
                        horseDragTrigger
                    );
                }
            }
        }

        // Dad becomes silent when the horse appears.
        if (dadArgumentSource != null)
            dadArgumentSource.Stop();

        // Begin the bonus music.
        if (fightMusicSource != null)
        {
            fightMusicSource.Stop();
            fightMusicSource.clip = bonusFightMusic;
            fightMusicSource.loop = true;

            if (bonusFightMusic != null)
                fightMusicSource.Play();
        }

        bonusMatchActive = true;
        runningSequence = false;

        // FightRoundManager keeps the screen covered while it
        // resets both fighters, then performs the round-intro fade.
        if (roundManager != null)
        {
            roundManager.BeginBonusMatch(
                horse,
                "HORSE"
            );
        }
    }

    private IEnumerator PlayFinale()
    {
        if (horse == null || farmer == null)
        {
            Debug.LogError(
                "Horse or Farmer is not assigned.",
                this
            );

            yield break;
        }

        if (dragEndPoint == null)
        {
            Debug.LogError(
                "Drag End Point is not assigned.",
                this
            );

            yield break;
        }

        if (farmerDragRoot == null)
        {
            Debug.LogWarning(
                "Farmer Drag Root is not assigned. " +
                "Using the Farmer FightCharacter transform.",
                this
            );

            farmerDragRoot = farmer.transform;
        }

        // Both characters stop fighting.
        horse.SetRoundActive(false);
        farmer.SetRoundActive(false);

        FightCharacterAI horseAI =
            horse.GetComponent<FightCharacterAI>();

        if (horseAI == null)
        {
            horseAI =
                horse.GetComponentInChildren<FightCharacterAI>(
                    true
                );
        }

        if (horseAI != null)
            horseAI.enabled = false;

        FighterInput farmerInput =
            farmer.GetComponent<FighterInput>();

        if (farmerInput == null)
        {
            farmerInput =
                farmer.GetComponentInChildren<FighterInput>(
                    true
                );
        }

        if (farmerInput != null)
            farmerInput.enabled = false;

        // The ordinary loss state now contains the knocked-down clip.
        // Give it time to settle before dragging begins.
        yield return new WaitForSecondsRealtime(
            dragStartDelay
        );

        // Freeze the knocked-down pose.
        if (farmerAnimator != null)
        {
            farmerAnimator.applyRootMotion = false;
            farmerAnimator.speed = 0f;
        }

        Rigidbody horseBody =
            horse.GetComponent<Rigidbody>();

        Rigidbody farmerBody =
            farmerDragRoot.GetComponent<Rigidbody>();

        if (horseBody == null)
        {
            horseBody =
                horse.GetComponentInChildren<Rigidbody>(
                    true
                );
        }

        if (farmerBody == null)
        {
            farmerBody =
                farmerDragRoot.GetComponentInChildren<Rigidbody>(
                    true
                );
        }

        // Disable physics so it does not override scripted movement.
        if (horseBody != null)
        {
            horseBody.linearVelocity = Vector3.zero;
            horseBody.angularVelocity = Vector3.zero;
            horseBody.useGravity = false;
            horseBody.isKinematic = true;
        }

        if (farmerBody != null)
        {
            farmerBody.linearVelocity = Vector3.zero;
            farmerBody.angularVelocity = Vector3.zero;
            farmerBody.useGravity = false;
            farmerBody.isKinematic = true;
        }

        // Prevent the farmer from catching on the floor.
        Collider[] farmerColliders =
            farmerDragRoot.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider farmerCollider in farmerColliders)
        {
            if (farmerCollider != null)
                farmerCollider.enabled = false;
        }

        // Begin the horse's dragging animation.
        if (horseAnimator != null &&
            !string.IsNullOrWhiteSpace(horseDragTrigger))
        {
            horseAnimator.speed = 1f;
            horseAnimator.ResetTrigger(horseDragTrigger);
            horseAnimator.SetTrigger(horseDragTrigger);
        }

        Vector3 horseStartPosition =
            horse.transform.position;

        Vector3 horseEndPosition =
            dragEndPoint.position;

        // Preserve the farmer's current position relative to the horse.
        Vector3 farmerOffset =
            farmerDragRoot.position -
            horseStartPosition;

        // Preserve the farmer's knocked-down rotation.
        Quaternion farmerRotation =
            farmerDragRoot.rotation;

        float elapsed = 0f;

        while (elapsed < dragDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / dragDuration
                );

            float smoothedProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            Vector3 newHorsePosition =
                Vector3.Lerp(
                    horseStartPosition,
                    horseEndPosition,
                    smoothedProgress
                );

            // Move the horse.
            horse.transform.position =
                newHorsePosition;

            // Directly move the farmer by the same amount.
            farmerDragRoot.position =
                newHorsePosition + farmerOffset;

            farmerDragRoot.rotation =
                farmerRotation;

            yield return null;
        }

        horse.transform.position =
            horseEndPosition;

        farmerDragRoot.position =
            horseEndPosition + farmerOffset;

        farmerDragRoot.rotation =
            farmerRotation;

        yield return FadeTo(1f);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeCanvas == null)
            yield break;

        float startAlpha = fadeCanvas.alpha;
        float elapsed = 0f;

        fadeCanvas.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadeCanvas.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                elapsed / fadeDuration
            );

            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
        fadeCanvas.blocksRaycasts =
            targetAlpha > 0f;
    }
}