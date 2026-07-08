using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Pause Screens")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseHomeScreen;
    [SerializeField] private GameObject optionsScreen;

    [Header("Menu References")]
    [SerializeField] private OptionsMenuController optionsMenu;
    [SerializeField] private Selectable pauseFirstSelected;

    public bool IsPaused { get; private set; }

    private Coroutine selectionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetPaused(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsPaused)
        {
            if (PauseButtonPressed())
                SetPaused(true);

            return;
        }

        if (CancelButtonPressed())
            HandleCancel();
    }

    private bool PauseButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetKeyDown(KeyCode.JoystickButton7);
    }

    private bool CancelButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetButtonDown("Cancel");
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void OpenOptions()
    {
        if (!IsPaused)
            return;

        if (pauseHomeScreen != null)
            pauseHomeScreen.SetActive(false);

        if (optionsScreen != null)
            optionsScreen.SetActive(true);

        if (optionsMenu != null)
            optionsMenu.OpenOptionsMenu();
    }

    public void ShowPauseHome()
    {
        if (optionsMenu != null)
            optionsMenu.CloseOptionsMenu();

        if (optionsScreen != null)
            optionsScreen.SetActive(false);

        if (pauseHomeScreen != null)
            pauseHomeScreen.SetActive(true);

        SelectNextFrame(pauseFirstSelected);
    }

    public void BackToPauseHome()
    {
        ShowPauseHome();
    }

    public void HandleCancel()
    {
        if (!IsPaused)
        {
            SetPaused(true);
            return;
        }

        if (optionsScreen != null &&
            optionsScreen.activeInHierarchy)
        {
            if (optionsMenu != null &&
                optionsMenu.TryLeaveSettings())
            {
                return;
            }

            ShowPauseHome();
            return;
        }

        ResumeGame();
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        if (paused)
        {
            ShowPauseHome();
        }
        else
        {
            if (optionsMenu != null)
                optionsMenu.CloseOptionsMenu();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void LoadMainMenu(string sceneName)
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        SetPaused(false);
    }

    private void SelectNextFrame(Selectable selectable)
    {
        if (selectionCoroutine != null)
            StopCoroutine(selectionCoroutine);

        selectionCoroutine = StartCoroutine(
            SelectAfterFrame(selectable)
        );
    }

    private IEnumerator SelectAfterFrame(
        Selectable selectable)
    {
        yield return null;

        if (selectable == null ||
            EventSystem.current == null ||
            !selectable.gameObject.activeInHierarchy)
        {
            selectionCoroutine = null;
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            selectable.gameObject
        );

        selectionCoroutine = null;
    }
}