using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [Tooltip("Small delay after pressing any button before fighter control turns on")]
    [SerializeField] private float startInputLockoutTime = 0.25f;

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

    [Tooltip("Icon used to display the player's round wins")]
    [SerializeField] private Image[] playerWinIcons;

    [Tooltip("Icon used to display the enemy's round wins")]
    [SerializeField] private Image[] enemyWinIcons;

    [Tooltip("Text used to display the player's name on the match screen")]
    [SerializeField] private GameObject playerNameText;

    [Tooltip("Text used to display the enemy's name on the match screen")]
    [SerializeField] private GameObject enemyNameText;

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

    private void ShowStartScreen()
    {
        CancelInvoke(nameof(StartRound));

        waitingForStart = true;
        waitingForNextRound = false;
        waitingForGameOverInput = false;
        startingRound = false;
        roundActive = false;
        gameOver = false;

        ResetMatchState();
        ResetFighters();
        SetFightersActive(false);
        UpdateAllUI();

        SetStartScreenVisible(true);
        SetNextRoundPromptVisible(false);
        SetNameTextVisible(false);
        SetRoundMessage("");

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
        if (!startingRound && Input.anyKeyDown)
            StartMatch();
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
        waitingForStart = false;
        startingRound = true;

        SetNameTextVisible(true);
        SetStartScreenVisible(false);
        SetNextRoundPromptVisible(false);
        QueueRoundStart();
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
        SetFightersActive(false);
        Invoke(nameof(StartRound), startInputLockoutTime);
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

        ResetFighters();
        SetFightersActive(true);
        UpdateAllUI();

        SetNextRoundPromptVisible(false);
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

        roundActive = false;
        SetFightersActive(false);

        if (roundWinner == playerCharacter)
            playerRoundWins++;
        else if (roundWinner == enemyCharacter)
            enemyRoundWins++;

        UpdateRoundWinText();

        if (CheckForGameWinner())
            return;

        currentRoundNumber++;
        ShowNextRoundPrompt();
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

        inputDelayTimer = nextRoundInputDelay;

        SetFightersActive(false);
        SetNextRoundPromptVisible(false);

        Debug.Log("Game ended");
    }

    private void ResetFighters()
    {
        if (playerHealth != null)
            playerHealth.ResetHealth();

        if (enemyHealth != null)
            enemyHealth.ResetHealth();

        ResetFighterPosition(playerCharacter, playerRigidbody, playerStartPoint);
        ResetFighterPosition(enemyCharacter, enemyRigidbody, enemyStartPoint);
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
                crownIcons[i].gameObject.SetActive(i < wins);
        }
    }

    private void SetRoundMessage(string message)
    {
        if (roundMessageText != null)
            roundMessageText.text = message;
    }

    private float TickTimer(float timer)
    {
        return Mathf.Max(0f, timer - Time.deltaTime);
    }
}