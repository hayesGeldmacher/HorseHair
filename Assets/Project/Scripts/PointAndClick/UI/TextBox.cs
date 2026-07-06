using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextBox : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float TextCrawlCooldown = 0.0f;

    [Range(10, 60)]
    [SerializeField] private int baseCharsPerSound; //how many characters type before audio plays - HG

    [Range(0, 10)]
    [SerializeField] private int charsVariance; //range of random varianace in chars before audio plays -HG

    public bool completeTextCrawl = true;
    private Coroutine CrawlCouroutine;
    private Coroutine _fadeCoroutine;
    private string textStorage = "";

    private void Start()
    {
        fadeGroup.alpha = 0;
    }

    public void SetName(string name)
    {
        _nameText.text = name;
    }

    public void SetText(string text)
    {
        _text.text = text;
        textStorage = text;
    }

    public void ClearText()
    {
        _text.text = string.Empty;
        textStorage = string.Empty;
    }

    public void ShowTextBox()
    {
        StartFade(1f);
    }

    public void ShowTextBoxTextCrawl(float speed, DialogueSound sound)
    {
        completeTextCrawl = false;
        CrawlCouroutine = StartCoroutine(CrawlText(speed, sound));
    }

    private IEnumerator CrawlText(float speed, DialogueSound sound)
    {
        StartFade(1f);
        float totalTime = _text.text.Length / speed;
        char[] textChars = _text.text.ToCharArray();
        float elapsedTime = 0f;
        _text.text = string.Empty;

        yield return new WaitUntil(() => fadeGroup.alpha >= 1f);

        int playedCharacters = 0;
        int totalCharacters = 0;
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / totalTime);
            _text.text = new string(textChars, 0, Mathf.FloorToInt(alpha * textChars.Length));
           
            //play dialogue sound based on how many characters hve been typed to screen - HG
            playedCharacters += (_text.text.Length - totalCharacters);
            
            int totalChars = baseCharsPerSound + (UnityEngine.Random.Range(-charsVariance, charsVariance));

            if(playedCharacters >= totalChars)
            {
                AudioManager.instance.PlayDialogueSound(sound);
                playedCharacters = 0;
                totalCharacters = _text.text.Length;
            }

            yield return null;
        }
        _text.text = textStorage;

        yield return new WaitForSeconds(TextCrawlCooldown);

        completeTextCrawl = true;
        CrawlCouroutine = null;
    }

    public void ShowTextBoxInstant()
    {
        if (CrawlCouroutine != null)
        {
            StopCoroutine(CrawlCouroutine);
            CrawlCouroutine = null;
        }

        fadeGroup.alpha = 1;
        _text.text = textStorage;
    }

    public void HideTextBox()
    {
        StartFade(0f);
    }

    public void HideTextBoxInstant()
    {
        fadeGroup.alpha = 0;
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
