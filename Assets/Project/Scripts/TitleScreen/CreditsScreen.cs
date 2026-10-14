using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform creditsContent;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private AudioSource music;

    [Header("Scrolling")]
    [SerializeField, Min(1f)] private float scrollSpeed = 50f;
    [SerializeField, Min(1f)] private float fastMultiplier = 4f;
    [SerializeField, Min(0f)] private float edgePadding = 20f;

    [Header("Transition")]
    [SerializeField, Min(0f)] private float blackHoldDuration = 2f;
    [SerializeField, Min(0f)] private float fadeDuration = 2f;
    [SerializeField] private string titleSceneName = "TitleScreen";

    private float musicVolume;
    private readonly Vector3[] contentCorners = new Vector3[4];

    private void Awake()
    {
        if (creditsContent == null || viewport == null ||
            blackOverlay == null || music == null)
        {
            Debug.LogError(
                "Assign all creditsscreen references in the Inspector.",
                this
            );

            enabled = false;
            return;
        }

        musicVolume = music.volume;
        music.Stop();
        music.volume = 0f;

        blackOverlay.alpha = 1f;
    }

    private IEnumerator Start()
    {
        Canvas.ForceUpdateCanvases();

        yield return new WaitForSecondsRealtime(blackHoldDuration);

        music.Play();
        yield return Fade(1f, 0f, 0f, musicVolume);

        while (!AllCreditsHavePassed())
        {
            bool speedingUp =
                (Mouse.current != null &&
                 Mouse.current.leftButton.isPressed) ||
                (Gamepad.current != null &&
                 Gamepad.current.buttonSouth.isPressed);

            float speed = scrollSpeed *
                (speedingUp ? fastMultiplier : 1f);

            creditsContent.position +=
                viewport.TransformVector(
                    Vector3.up * speed * Time.unscaledDeltaTime
                );

            yield return null;
        }

        yield return Fade(0f, 1f, musicVolume, 0f);

        music.Stop();
        yield return SceneManager.LoadSceneAsync(titleSceneName);
    }

    private bool AllCreditsHavePassed()
    {
        float lowestY = float.PositiveInfinity;
        bool foundContent = false;

        foreach (RectTransform child in
                 creditsContent.GetComponentsInChildren<RectTransform>())
        {
            if (child == creditsContent || !child.gameObject.activeInHierarchy)
                continue;

            TMP_Text text = child.GetComponent<TMP_Text>();

            if (text != null)
            {
                text.ForceMeshUpdate();

                TMP_TextInfo info = text.textInfo;

                for (int i = 0; i < info.characterCount; i++)
                {
                    TMP_CharacterInfo character = info.characterInfo[i];

                    if (!character.isVisible)
                        continue;

                    IncludePoint(child, character.bottomLeft, ref lowestY);
                    IncludePoint(child, character.bottomRight, ref lowestY);
                    foundContent = true;
                }
            }
            else if (child.GetComponent<UnityEngine.UI.Graphic>() != null)
            {
                child.GetWorldCorners(contentCorners);

                foreach (Vector3 corner in contentCorners)
                {
                    float y = viewport.InverseTransformPoint(corner).y;
                    lowestY = Mathf.Min(lowestY, y);
                }

                foundContent = true;
            }
        }

        return !foundContent ||
               lowestY > viewport.rect.yMax + edgePadding;
    }

    private void IncludePoint(
        RectTransform rect,
        Vector3 localPoint,
        ref float lowestY)
    {
        float y = viewport.InverseTransformPoint(
            rect.TransformPoint(localPoint)
        ).y;

        lowestY = Mathf.Min(lowestY, y);
    }

    private IEnumerator Fade(
        float startAlpha,
        float endAlpha,
        float startVolume,
        float endVolume)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            blackOverlay.alpha =
                Mathf.Lerp(startAlpha, endAlpha, t);

            music.volume =
                Mathf.Lerp(startVolume, endVolume, t);

            yield return null;
        }

        blackOverlay.alpha = endAlpha;
        music.volume = endVolume;
    }
}