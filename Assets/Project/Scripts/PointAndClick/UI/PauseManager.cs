using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        // Prevent duplicate pause systems.
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

    private void Update()
    {
        // Legacy Input Manager example.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPaused(false);
    }
}