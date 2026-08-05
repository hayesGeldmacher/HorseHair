using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles start screen, round timing, round wins, bo3 game rules, next-round prompt, and round resets
/// </summary>
public class FightRoundManager : MonoBehaviour
{
    [Header("Start Screen")]
    [Tooltip("Panel shown before the fighting game starts")]
    [SerializeField] private GameObject startScreenPanel;

    [Tooltip("Title text object shown on the start screen")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("Press any button text object shown on the start screen")]
    [SerializeField] private TMP_Text pressAnyButtonText;

    [Tooltip("How fast the press any button text blinks")]
    [SerializeField] private float blinkSpeed = 2f;

    [Header("Character Select")]
    [Tooltip("Character selection shown after leaving the start screen")]
    [SerializeField] private CharacterSelectManager characterSelectManager;

    [Tooltip("How long the loading screen stays up before character selection")]
    [SerializeField] private float characterSelectLoadingTime = 1f;

    [Header("Next Round Prompt")]
    [Tooltip("Text shown between rounds to wait for player input")]
    [SerializeField] private TMP_Text nextRoundPromptText;

    [Header("End Game Prompt")]
    [Tooltip("Text shown when the game is over to prompt the player to return to the start screen")]
    [SerializeField] private TMP_Text gameOverPromptText;

    [Tooltip("How long to ignore input after a round ends before the next-round prompt can be accepted")]
    [SerializeField] private float nextRoundInputDelay = 1f;

    [Header("Round Rules")]
    [Tooltip("Length of each round in seconds")]
    [SerializeField] private float roundTimeSeconds = 60f;

    [Tooltip("Number of round wins needed to win the full game")]
    [SerializeField] private int roundsNeededToWinGame = 2;

    [Header("Fighter References")]
    [Tooltip("Player FightCharacter component")]
    [SerializeField] private FightCharacter playerCharacter;

    [Tooltip("Enemy FightCharacter component")]
    [SerializeField] private FightCharacter enemyCharacter;

    [Tooltip("Player health component")]
    [SerializeField] private FighterHealth playerHealth;

    [Tooltip("Enemy health component")]
    [SerializeField] private FighterHealth enemyHealth;

    [Header("Super Meter References")]

    [Tooltip("Player super meter component")]
    [SerializeField] private FighterSuperMeter playerSuperMeter;

    [Tooltip("Enemy super meter component")]
    [SerializeField] private FighterSuperMeter enemySuperMeter;

    [Header("Round Reset Positions")]
    [Tooltip("Player position at the start of each round")]
    [SerializeField] private Transform playerStartPoint;

    [Tooltip("Enemy position at the start of each round")]
    [SerializeField] private Transform enemyStartPoint;

    [Tooltip("Player Rigidbody")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Tooltip("Enemy Rigidbody")]
    [SerializeField] private Rigidbody enemyRigidbody;

    [Header("UI")]
    [Tooltip("Text used to display the round timer")]
    [SerializeField] private TMP_Text timerText;

    [Tooltip("Text used to display round and game messages")]
    [SerializeField] private TMP_Text roundMessageText;

    [Tooltip("Text used to display center-screen round begin and victor messages")]
    [SerializeField] private TMP_Text centerMessageText;

    [Tooltip("Icon used to display the player's round wins")]
    [SerializeField] private Image[] playerWinIcons;

    [Tooltip("Icon used to display the enemy's round wins")]
    [SerializeField] private Image[] enemyWinIcons;

    [Tooltip("Sprite shown before a round has been won")]
    [SerializeField] private Sprite eggSprite;

    [Tooltip("Sprite shown after a round has been won")]
    [SerializeField] private Sprite hatchedSprite;

    [Tooltip("Text used to display the player's name on the match screen")]
    [SerializeField] private GameObject playerNameText;

    [Tooltip("Text used to display the enemy's name on the match screen")]
    [SerializeField] private GameObject enemyNameText;

    [Header("Screen Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Round Intro")]
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private float roundIntroBlackTime = 1f;
    [SerializeField] private float roundBeginTextTime = 2f;

    [Header("Round End Presentation")]
    [SerializeField] private float roundEndSlowMotionScale = 0.25f;
    [SerializeField] private float roundEndPresentationTime = 4f;
    [SerializeField] private string playerDisplayName = "Player";
    [SerializeField] private string enemyDisplayName = "Enemy";

    [Header("Scene Transition")]
    [SerializeField] private EyelidsFG eyelids;
    [SerializeField] private string nextSceneName;
    private bool triggeredTransition = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource startScreenNoiseSource;
    [SerializeField] private AudioClip startScreenWhiteNoise;
    [SerializeField] private AudioClip startButtonClickSFX;
    [SerializeField] private AudioClip roundWinSFX;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip matchWonSFX;

    private float currentRoundTime;
    private float startBlinkTimer;
    private float nextRoundBlinkTimer;
    private float inputDelayTimer;

    private int playerRoundWins;
    private int enemyRoundWins;
    private int currentRoundNumber = 1;

    private bool waitingForStart;
    private bool waitingForNextRound;
    private bool waitingForGameOverInput;
    private bool startingRound;
    private bool roundActive;
    private bool gameOver;

    private Coroutine roundIntroCoroutine;
    private Coroutine roundEndCoroutine;
    private Coroutine characterSelectLoadingCoroutine;

    [SerializeField] private bool gameCanStart = false;

    private void Start()
    {
        ShowStartScreen();
    }

    private void Update()
    {
        if (waitingForStart)
        {
            UpdateStartScreenBlink();
            CheckForAnyButtonStart();
            return;
        }

        if (waitingForNextRound)
        {
            UpdateNextRoundPrompt();
            CheckForAnyButtonNextRound();
            return;
        }

        if (waitingForGameOverInput)
        {
            inputDelayTimer = TickTimer(inputDelayTimer);
            CheckForAnyButtonGameOver();
            return;
        }

        if (!roundActive || gameOver)
            return;

        UpdateRoundTimer();
        CheckForRoundWinner();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SetFighterPresentationVisible(true);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return;
        }

        Debug.Log("Playing clip: " + clip.name);
        audioSource.PlayOneShot(clip, 2f);
    }

    private void ShowStartScreen()
    {
        CancelInvoke(nameof(StartRound));

        if (roundIntroCoroutine != null)
            StopCoroutine(roundIntroCoroutine);

        if (roundEndCoroutine != null)
            StopCoroutine(roundEndCoroutine);

        if (characterSelectLoadingCoroutine != null)
        {
            StopCoroutine(characterSelectLoadingCoroutine);
            characterSelectLoadingCoroutine = null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        waitingForStart = true;
        waitingForNextRound = false;
        waitingForGameOverInput = false;
        startingRound = false;
        roundActive = false;
        gameOver = false;
        triggeredTransition = false;

        ResetMatchState();
        ResetFighters();
        SetFighterPresentationVisible(true);
        SetFightersActive(false);
        UpdateAllUI();

        SetStartScreenVisible(true);
        if (characterSelectManager != null)
            characterSelectManager.HideAndReset();

        SetNextRoundPromptVisible(false);
        SetNameTextVisible(false);
        SetRoundWinIconsVisible(false);
        SetRoundMessage("");
        SetCenterMessage("");

        if (gameOverPromptText != null)
            gameOverPromptText.gameObject.SetActive(false);

        if (loadingIcon != null)
            loadingIcon.SetActive(false);

        if (startScreenNoiseSource != null && startScreenWhiteNoise != null)
        {
            startScreenNoiseSource.clip = startScreenWhiteNoise;
            startScreenNoiseSource.loop = true;
            startScreenNoiseSource.Play();
        }

        Debug.Log("Start screen shown");
    }

    private void ResetMatchState()
    {
        currentRoundNumber = 1;
        playerRoundWins = 0;
        enemyRoundWins = 0;
        currentRoundTime = roundTimeSeconds;
        inputDelayTimer = 0f;
    }

    private void CheckForAnyButtonStart()
    {

        if (Input.GetKeyDown(KeyCode.Tab) || !gameCanStart)
            return;

        if (!startingRound && Input.anyKeyDown)
        {
            if (startScreenNoiseSource != null && startButtonClickSFX != null)
                startScreenNoiseSource.PlayOneShot(startButtonClickSFX, 2f);

            if (characterSelectManager != null)
            {
                waitingForStart = false;
                characterSelectLoadingCoroutine = StartCoroutine(
                    CharacterSelectLoadingRoutine()
                );
            }
            else
            {
                StartMatch();
            }
        }
    }

    private void CheckForAnyButtonNextRound()
    {
        if (!startingRound && inputDelayTimer <= 0f && Input.anyKeyDown)
            StartNextRound();
    }

    private void CheckForAnyButtonGameOver()
    {
        if (inputDelayTimer <= 0f && Input.anyKeyDown)
            ShowStartScreen();
    }

    private void StartMatch()
    {
        StartCoroutine(StartMatchFadeRoutine());
    }

    private IEnumerator CharacterSelectLoadingRoutine()
    {
        startingRound = true;

        yield return FadeScreen(1f);

        SetStartScreenVisible(false);

        if (startScreenNoiseSource != null)
        {
            startScreenNoiseSource.Stop();
            startScreenNoiseSource.loop = false;
            startScreenNoiseSource.clip = null;
        }

        if (loadingIcon != null)
            loadingIcon.SetActive(true);

        yield return new WaitForSecondsRealtime(characterSelectLoadingTime);

        if (loadingIcon != null)
            loadingIcon.SetActive(false);

        startingRound = false;
        characterSelectManager.OpenSelection(fadeDuration);

        yield return FadeScreen(0f);

        characterSelectLoadingCoroutine = null;
    }

    public void StartMatchFromCharacterSelect()
    {
        if (startingRound || roundActive || gameOver)
            return;

        StartMatch();
    }

    private IEnumerator StartMatchFadeRoutine()
    {
        waitingForStart = false;
        startingRound = true;

        yield return FadeScreen(1f);

        if (characterSelectManager != null)
            characterSelectManager.HideAndReset();

        if (loadingIcon != null)
            loadingIcon.SetActive(true);

        yield return new WaitForSeconds(1f);

        if (startScreenNoiseSource != null)
        {
            startScreenNoiseSource.Stop();
            startScreenNoiseSource.loop = false;
            startScreenNoiseSource.clip = null;
        }

        SetStartScreenVisible(false);
        SetNameTextVisible(true);
        SetRoundWinIconsVisible(true);
        SetNextRoundPromptVisible(false);

        QueueRoundStart();

        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        fadeCanvasGroup.blocksRaycasts = true;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0f;
    }

    private void StartNextRound()
    {
        waitingForNextRound = false;
        startingRound = true;

        SetNextRoundPromptVisible(false);
        QueueRoundStart();
    }

    private void QueueRoundStart()
    {
        CancelInvoke(nameof(StartRound));

        if (roundIntroCoroutine != null)
            StopCoroutine(roundIntroCoroutine);

        roundIntroCoroutine = StartCoroutine(RoundIntroRoutine());
    }

    private IEnumerator RoundIntroRoutine()
    {
        SetFightersActive(false);
        startingRound = true;
        roundActive = false;

        SetCenterMessage("");

        if (loadingIcon != null)
            loadingIcon.SetActive(true);

        yield return FadeScreen(1f);

        SetFighterPresentationVisible(false);

        ResetFighters();
        currentRoundTime = roundTimeSeconds;
        UpdateAllUI();
        SetRoundMessage("Round " + currentRoundNumber);

        yield return new WaitForSeconds(roundIntroBlackTime);

        SetFighterPresentationVisible(true);

        if (loadingIcon != null)
            loadingIcon.SetActive(false);

        yield return FadeScreen(0f);

        SetCenterMessage("ROUND " + currentRoundNumber + ", BEGIN!");

        yield return new WaitForSeconds(roundBeginTextTime);

        StartRound();
    }

    private void ShowNextRoundPrompt()
    {
        waitingForNextRound = true;
        startingRound = false;
        roundActive = false;

        inputDelayTimer = nextRoundInputDelay;
        nextRoundBlinkTimer = 0f;

        SetFightersActive(false);
        SetNextRoundPromptVisible(true);
    }

    private void UpdateStartScreenBlink()
    {
        BlinkText(pressAnyButtonText, ref startBlinkTimer);
    }

    private void UpdateNextRoundPrompt()
    {
        inputDelayTimer = TickTimer(inputDelayTimer);
        BlinkText(nextRoundPromptText, ref nextRoundBlinkTimer);
    }

    private void BlinkText(TMP_Text text, ref float timer)
    {
        if (text == null)
            return;

        timer += Time.deltaTime * blinkSpeed;
        text.enabled = Mathf.Sin(timer) > 0f;
    }

    private void StartRound()
    {
        if (waitingForStart || gameOver)
            return;

        waitingForNextRound = false;
        startingRound = false;
        roundActive = true;
        currentRoundTime = roundTimeSeconds;

        SetFightersActive(true);
        UpdateAllUI();

        SetNextRoundPromptVisible(false);
        SetCenterMessage("");
        SetRoundMessage("Round " + currentRoundNumber);

        Debug.Log("Round " + currentRoundNumber + " started.");
    }

    private void UpdateRoundTimer()
    {
        currentRoundTime = TickTimer(currentRoundTime);
        UpdateTimerText();

        if (currentRoundTime <= 0f)
            EndRoundByTime();
    }

    private void CheckForRoundWinner()
    {
        if (playerHealth != null && playerHealth.IsDefeated)
        {
            EndRound(enemyCharacter);
        }
        else if (enemyHealth != null && enemyHealth.IsDefeated)
        {
            EndRound(playerCharacter);
        }
    }

    /// <summary>
    /// Higher health wins. If health is equal, the player wins the round.
    /// </summary>
    private void EndRoundByTime()
    {
        if (playerHealth == null || enemyHealth == null)
            return;

        FightCharacter roundWinner = playerHealth.GetCurrentHealth() >= enemyHealth.GetCurrentHealth()
            ? playerCharacter
            : enemyCharacter;

        EndRound(roundWinner);
    }

    private void EndRound(FightCharacter roundWinner)
    {
        if (!roundActive)
            return;

        if (roundEndCoroutine != null)
            StopCoroutine(roundEndCoroutine);

        roundEndCoroutine = StartCoroutine(RoundEndRoutine(roundWinner));
    }

    private IEnumerator RoundEndRoutine(FightCharacter roundWinner)
    {
        roundActive = false;
        SetRoundResults(roundWinner);

        if (roundWinner == playerCharacter)
            playerRoundWins++;
        else if (roundWinner == enemyCharacter)
            enemyRoundWins++;

        if (playerRoundWins >= roundsNeededToWinGame ||
            enemyRoundWins >= roundsNeededToWinGame)
        {
            UpdateRoundWinText();
        }

        Time.timeScale = roundEndSlowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        string winnerName = roundWinner == playerCharacter ? playerDisplayName : enemyDisplayName;
        SetCenterMessage(winnerName + " is VICTOR!");

        PlaySound(roundWinSFX);

        yield return new WaitForSecondsRealtime(roundEndPresentationTime);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SetCenterMessage("");

        if (CheckForGameWinner())
            yield break;

        currentRoundNumber++;
        ShowNextRoundPrompt();
    }

    private void SetRoundResults(FightCharacter roundWinner)
    {
        FightCharacter roundLoser = roundWinner == playerCharacter
            ? enemyCharacter
            : playerCharacter;

        if (roundWinner != null)
            roundWinner.SetRoundResult(true);

        if (roundLoser != null)
            roundLoser.SetRoundResult(false);
    }

    private bool CheckForGameWinner()
    {
        if (playerRoundWins < roundsNeededToWinGame && enemyRoundWins < roundsNeededToWinGame)
            return false;

        EndGame();
        return true;
    }

    private void EndGame()
    {
        gameOver = true;
        roundActive = false;
        waitingForNextRound = false;
        waitingForGameOverInput = true;
        startingRound = false;

        if (gameOverPromptText != null)
            gameOverPromptText.gameObject.SetActive(true);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        PlaySound(matchWonSFX);

        inputDelayTimer = nextRoundInputDelay;

        SetFightersActive(false);
        SetNextRoundPromptVisible(false);

        if (!triggeredTransition)
        {
            triggeredTransition = true;
            StartCoroutine(TransitionScene());
        }
    }

    private IEnumerator TransitionScene()
    {
        eyelids.TriggerEyesDownAnimation();
        yield return new WaitForSeconds(2.0f);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene("SCN_DreamSequenceN1");
    }

    private void ResetFighters()
    {
        if (playerHealth != null)
            playerHealth.ResetHealth();

        if (enemyHealth != null)
            enemyHealth.ResetHealth();

        if (playerSuperMeter != null)
            playerSuperMeter.ResetSuper();

        if (enemySuperMeter != null)
            enemySuperMeter.ResetSuper();

        ResetFighterPosition(playerCharacter, playerRigidbody, playerStartPoint);
        ResetFighterPosition(enemyCharacter, enemyRigidbody, enemyStartPoint);

        if (playerCharacter != null)
            playerCharacter.ResetRoundState();

        if (enemyCharacter != null)
            enemyCharacter.ResetRoundState();
    }

    private void ResetFighterPosition(FightCharacter fighter, Rigidbody fighterRigidbody, Transform startPoint)
    {
        if (fighter == null || startPoint == null)
            return;

        if (fighterRigidbody != null)
        {
            fighterRigidbody.linearVelocity = Vector3.zero;
            fighterRigidbody.angularVelocity = Vector3.zero;
            fighterRigidbody.position = startPoint.position;
            fighterRigidbody.rotation = startPoint.rotation;
            return;
        }

        fighter.transform.position = startPoint.position;
        fighter.transform.rotation = startPoint.rotation;
    }

    private void SetFightersActive(bool isActive)
    {
        if (playerCharacter != null)
            playerCharacter.SetRoundActive(isActive);

        if (enemyCharacter != null)
            enemyCharacter.SetRoundActive(isActive);
    }

    private void SetFighterPresentationVisible(bool isVisible)
    {
        if (playerCharacter != null)
            playerCharacter.SetPresentationVisible(isVisible);

        if (enemyCharacter != null)
            enemyCharacter.SetPresentationVisible(isVisible);
    }

    private void SetStartScreenVisible(bool isVisible)
    {
        if (startScreenPanel != null)
            startScreenPanel.SetActive(isVisible);

        if (titleText != null)
            titleText.gameObject.SetActive(isVisible);

        if (pressAnyButtonText != null)
        {
            pressAnyButtonText.gameObject.SetActive(isVisible);
            pressAnyButtonText.enabled = isVisible;
        }
    }

    private void SetNextRoundPromptVisible(bool isVisible)
    {
        if (nextRoundPromptText == null)
            return;

        nextRoundPromptText.gameObject.SetActive(isVisible);
        nextRoundPromptText.enabled = isVisible;
    }

    private void SetNameTextVisible(bool isVisible)
    {
        if (playerNameText != null)
            playerNameText.SetActive(isVisible);

        if (enemyNameText != null)
            enemyNameText.SetActive(isVisible);
    }

    private void SetRoundWinIconsVisible(bool isVisible)
    {
        SetIconArrayVisible(playerWinIcons, isVisible);
        SetIconArrayVisible(enemyWinIcons, isVisible);
    }

    private void SetIconArrayVisible(Image[] icons, bool isVisible)
    {
        if (icons == null)
            return;

        foreach (Image icon in icons)
        {
            if (icon != null)
                icon.gameObject.SetActive(isVisible);
        }
    }

    private void UpdateAllUI()
    {
        UpdateTimerText();
        UpdateRoundWinText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        timerText.text = Mathf.CeilToInt(currentRoundTime).ToString();
    }

    private void UpdateRoundWinText()
    {
        UpdateCrownIcons(playerWinIcons, playerRoundWins);
        UpdateCrownIcons(enemyWinIcons, enemyRoundWins);
    }

    private void UpdateCrownIcons(Image[] crownIcons, int wins)
    {
        if (crownIcons == null)
            return;

        for (int i = 0; i < crownIcons.Length; i++)
        {
            if (crownIcons[i] != null)
                crownIcons[i].sprite = i < wins ? hatchedSprite : eggSprite;
        }
    }

    private void SetRoundMessage(string message)
    {
        if (roundMessageText != null)
            roundMessageText.text = message;
    }

    private void SetCenterMessage(string message)
    {
        if (centerMessageText != null)
            centerMessageText.text = message;
    }

    private float TickTimer(float timer)
    {
        return Mathf.Max(0f, timer - Time.deltaTime);
    }

    //called from TelevisionSequence to begin the fighting game - HG
    public void SetGameActive()
    {
        gameCanStart = true;
    }
}