using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenControls : MonoBehaviour
{
    [SerializeField] private string StartingLevel;
    [SerializeField] private AudioVolumeFade titleMusicFade;

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
            SceneManager.LoadScene(StartingLevel);
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

        if (titleMusicFade != null)
            titleMusicFade.StartFadeIn(false, false);

        yield return FadeHeadphones(1f, 0f);

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

    public void ContinueGame()
    {
        if (starting)
            return;

        SceneManager.LoadScene(PlayerPrefs.GetString("Scene"));
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