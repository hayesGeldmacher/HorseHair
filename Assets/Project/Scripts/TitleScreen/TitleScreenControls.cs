using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenControls : MonoBehaviour
{
    [SerializeField] private string StartingLevel;
    [SerializeField] private AudioSource titleMusicSource;

    [Header("Headphones Screen")]
    [SerializeField] private GameObject headphonesPanel;
    [SerializeField] private CanvasGroup headphonesContent;
    [SerializeField] private float displaySeconds = 5f;
    [SerializeField] private float fadeSeconds = 1f;
    [SerializeField] private CanvasGroup screenFade;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private GameObject newGameConfirmation;
    [SerializeField] private GameObject deleteGameConfirmation;

    private const string HeadphonesShownKey = "HeadphonesScreenShown";
    private bool starting;

    private void Awake()
    {
        if (newGameConfirmation != null)
            newGameConfirmation.SetActive(false);
        
        if (deleteGameConfirmation != null)
            deleteGameConfirmation.SetActive(false);

        if (headphonesPanel != null)
            headphonesPanel.SetActive(false);
    }

    private void BeginNewGame()
    {
        if (starting)
            return;

        starting = true;

        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
        PlayerPrefs.DeleteKey("Thoughts");
        PlayerPrefs.DeleteKey("Scene");
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

    public void StartGame()
    {
        if (starting)
            return;

        if (HasSaveData())
        {
            if (newGameConfirmation != null)
                newGameConfirmation.SetActive(true);

            return;
        }

        BeginNewGame();
    }

    public void ConfirmNewGame()
    {
        if (starting)
            return;

        if (newGameConfirmation != null)
           // newGameConfirmation.SetActive(false);

        BeginNewGame();
    }

    public void deleteGame()
    {
        if (starting)
            return;
        if (deleteGameConfirmation != null)
            deleteGameConfirmation.SetActive(true);
    }

    public void ConfirmDeleteGame()
    {
        if (starting)
            return;
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
        PlayerPrefs.DeleteKey("Thoughts");
        PlayerPrefs.DeleteKey("Scene");
        PlayerPrefs.Save();
        if (deleteGameConfirmation != null)
            deleteGameConfirmation.SetActive(false);
        if (resumeButton != null)
            resumeButton.interactable = HasSaveData();
    }

    public void CancelDeleteGame()
    {
        if (deleteGameConfirmation != null)
            deleteGameConfirmation.SetActive(false);
    }

    public void CancelNewGame()
    {
        if (newGameConfirmation != null)
            newGameConfirmation.SetActive(false);
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
        Coroutine screenFadeRoutine = StartCoroutine(FadeScreenToBlack());

        yield return FadeHeadphones(1f, 0f);
        yield return screenFadeRoutine;
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
        Coroutine musicFade = StartCoroutine(FadeTitleMusic());

        yield return FadeScreenToBlack();
        yield return musicFade;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeScreenToBlack()
    {
        if (screenFade == null)
            yield break;

        screenFade.blocksRaycasts = true;

        float startAlpha = screenFade.alpha;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            screenFade.alpha = Mathf.Lerp(
                startAlpha, 1f,
                Mathf.Clamp01(elapsed / fadeSeconds));

            yield return null;
        }

        screenFade.alpha = 1f;
    }

    private bool HasSaveData()
    {
        string savedScene = PlayerPrefs.GetString("Scene", "");

        return !string.IsNullOrEmpty(savedScene)
            && Application.CanStreamedLevelBeLoaded(savedScene);
    }

    private void OnEnable()
    {
        if (resumeButton != null)
            resumeButton.interactable = HasSaveData();
    }

    public void ContinueGame()
    {
        if (starting || !HasSaveData())
            return;

        starting = true;
        StartCoroutine(FadeAndLoad(PlayerPrefs.GetString("Scene")));
    }

    [ContextMenu("Clear Saved Scene For Testing")]
    private void ClearSavedSceneForTesting()
    {
        PlayerPrefs.DeleteKey("Scene");
        PlayerPrefs.Save();

        if (resumeButton != null)
            resumeButton.interactable = HasSaveData();
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