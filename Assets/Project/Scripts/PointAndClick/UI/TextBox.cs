using System.Collections;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine _fadeCoroutine;

    private void Start()
    {
        fadeGroup.alpha = 0;
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void ClearText()
    {
        _text.text = string.Empty;
    }

    public void ShowTextBox()
    {
        fadeGroup.alpha = 1;
    }

    public void HideTextBox()
    {
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}
