using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenControls : MonoBehaviour
{
    [SerializeField] private string StartingLevel;
    [SerializeField] private AudioSource titleMusicSource;

    [Header("Headphones Screen")]
    [SerializeField] private GameObject headphonesPanel;
    [SerializeField] private CanvasGroup headphonesContent;
    [SerializeField] private float displaySeconds = 5f;
    [SerializeField] private float fadeSeconds = 1f;

    private const string HeadphonesShownKey = "HeadphonesScreenShown";
    private bool starting;

    private void Awake()
    {
        if (headphonesPanel != null)
            headphonesPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (starting)
            return;

        starting = true;

        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
        PlayerPrefs.DeleteKey("Thoughts");
        PlayerPrefs.SetInt("TaskNum", 0);
        PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Morning);
        PlayerPrefs.Save();

        bool alreadyShown =
            PlayerPrefs.GetInt(HeadphonesShownKey, 0) == 1;

        if (alreadyShown ||
            headphonesPanel == null ||
            headphonesContent == null)
        {
            StartCoroutine(FadeAndLoad(StartingLevel));
            return;
        }

        StartCoroutine(ShowHeadphones());
    }

    private IEnumerator ShowHeadphones()
    {
        headphonesContent.alpha = 0f;
        headphonesPanel.SetActive(true);

        PlayerPrefs.SetInt(HeadphonesShownKey, 1);
        PlayerPrefs.Save();

        yield return FadeHeadphones(0f, 1f);

        yield return new WaitForSecondsRealtime(displaySeconds);

        Coroutine musicFade = StartCoroutine(FadeTitleMusic());

        yield return FadeHeadphones(1f, 0f);
        yield return musicFade;

        SceneManager.LoadScene(StartingLevel);
    }

    private IEnumerator FadeHeadphones(float from, float to)
    {
        if (fadeSeconds <= 0f)
        {
            headphonesContent.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            headphonesContent.alpha = Mathf.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / fadeSeconds)
            );

            yield return null;
        }

        headphonesContent.alpha = to;
    }

    private IEnumerator FadeTitleMusic()
    {
        float startVolume = titleMusicSource != null
            ? titleMusicSource.volume : 0f;

        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            if (titleMusicSource != null)
            {
                titleMusicSource.volume = Mathf.Lerp(
                    startVolume, 0f,
                    Mathf.Clamp01(elapsed / fadeSeconds));
            }

            yield return null;
        }

        if (titleMusicSource != null)
        {
            titleMusicSource.volume = 0f;
            titleMusicSource.Stop();
        }
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return FadeTitleMusic();
        SceneManager.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
        if (starting)
            return;

        starting = true;
        StartCoroutine(FadeAndLoad(PlayerPrefs.GetString("Scene")));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [ContextMenu("Reset Headphones Screen")]
    private void ResetHeadphonesScreen()
    {
        PlayerPrefs.DeleteKey(HeadphonesShownKey);
        PlayerPrefs.Save();
    }
}