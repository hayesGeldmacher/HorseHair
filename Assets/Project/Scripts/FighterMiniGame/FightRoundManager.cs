using UnityEngine;
using TMPro;

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

    [Tooltip("If true, the game returns to the start screen after the match ends")]
    [SerializeField] private bool returnToStartScreenAfterGameOver = true;

    [Header("Next Round Prompt")]
    [Tooltip("Text shown between rounds to wait for player input")]
    [SerializeField] private TMP_Text nextRoundPromptText;

    [Tooltip("How long to ignore input after a round ends before the next-round prompt can be accepted")]
    [SerializeField] private float nextRoundInputDelay = 1f;

    [Header("Round Rules")]
    [Tooltip("Length of each round in seconds 120 seconds = 2 minutes")]
    [SerializeField] private float roundTimeSeconds = 120f;

    [Tooltip("Number of round wins needed to win the full game")]
    [SerializeField] private int roundsNeededToWinGame = 2;

    [Tooltip("Delay before returning to the start screen after the full game ends")]
    [SerializeField] private float roundRestartDelay = 2f;

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

    [Tooltip("Text used to display the player's round wins")]
    [SerializeField] private TMP_Text playerRoundWinsText;

    [Tooltip("Text used to display the enemy's round wins")]
    [SerializeField] private TMP_Text enemyRoundWinsText;

    private float currentRoundTime;
    private int playerRoundWins;
    private int enemyRoundWins;
    private int currentRoundNumber = 1;

    private float startBlinkTimer;
    private float nextRoundBlinkTimer;
    private float nextRoundInputTimer;

    private bool waitingForStart;
    private bool waitingForNextRound;
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

        if (!roundActive || gameOver)
            return;

        UpdateRoundTimer();
        CheckForRoundWinner();
    }

    /// <summary>
    /// Shows the start screen and disables fighter control
    /// </summary>
    private void ShowStartScreen()
    {
        CancelInvoke(nameof(StartRound));

        waitingForStart = true;
        waitingForNextRound = false;
        startingRound = false;
        roundActive = false;
        gameOver = false;

        ResetMatchState();
        ResetFighters();
        SetFightersActive(false);
        UpdateAllUI();

        SetStartScreenVisible(true);
        SetNextRoundPromptVisible(false);

        startBlinkTimer = 0f;
        nextRoundBlinkTimer = 0f;
        nextRoundInputTimer = 0f;

        SetRoundMessage("");

        Debug.Log("Start screen shown");
    }

    private void ResetMatchState()
    {
        currentRoundNumber = 1;
        playerRoundWins = 0;
        enemyRoundWins = 0;
        currentRoundTime = roundTimeSeconds;
    }

    /// <summary>
    /// Checks for any button or key press to start the match
    /// </summary>
    private void CheckForAnyButtonStart()
    {
        if (startingRound || !Input.anyKeyDown)
            return;

        StartMatch();
    }

    /// <summary>
    /// Checks for any button or key press to start the next round
    /// </summary>
    private void CheckForAnyButtonNextRound()
    {
        if (startingRound || nextRoundInputTimer > 0f || !Input.anyKeyDown)
            return;

        StartNextRound();
    }

    /// <summary>
    /// Hides the start screen and starts the first round after a short input lockout
    /// </summary>
    private void StartMatch()
    {
        waitingForStart = false;
        waitingForNextRound = false;
        startingRound = true;
        gameOver = false;

        SetStartScreenVisible(false);
        SetNextRoundPromptVisible(false);
        QueueRoundStart();
    }

    /// <summary>
    /// Starts the next round after the between-round prompt
    /// </summary>
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

    /// <summary>
    /// Shows the next-round prompt and locks input briefly to prevent accidental button spam
    /// </summary>
    private void ShowNextRoundPrompt()
    {
        waitingForNextRound = true;
        startingRound = false;
        roundActive = false;

        nextRoundInputTimer = nextRoundInputDelay;
        nextRoundBlinkTimer = 0f;

        SetFightersActive(false);
        SetNextRoundPromptVisible(true);
    }

    /// <summary>
    /// Makes the start screen press-any-button text blink
    /// </summary>
    private void UpdateStartScreenBlink()
    {
        BlinkText(pressAnyButtonText, ref startBlinkTimer);
    }

    /// <summary>
    /// Updates the next-round prompt blink and input delay
    /// </summary>
    private void UpdateNextRoundPrompt()
    {
        nextRoundInputTimer = TickTimer(nextRoundInputTimer);
        BlinkText(nextRoundPromptText, ref nextRoundBlinkTimer);
    }

    private void BlinkText(TMP_Text text, ref float timer)
    {
        if (text == null)
            return;

        timer += Time.deltaTime * blinkSpeed;
        text.enabled = Mathf.Sin(timer) > 0f;
    }

    /// <summary>
    /// Starts or restarts a round
    /// </summary>
    private void StartRound()
    {
        if (waitingForStart || gameOver)
            return;

        waitingForNextRound = false;
        startingRound = false;
        currentRoundTime = roundTimeSeconds;
        roundActive = true;

        ResetFighters();
        SetFightersActive(true);
        UpdateAllUI();

        SetNextRoundPromptVisible(false);
        SetRoundMessage("Round " + currentRoundNumber);

        Debug.Log("Round " + currentRoundNumber + " started.");
    }

    /// <summary>
    /// Updates the round timer and ends the round when time reaches zero
    /// </summary>
    private void UpdateRoundTimer()
    {
        currentRoundTime = TickTimer(currentRoundTime);

        if (currentRoundTime <= 0f)
        {
            EndRoundByTime();
        }

        UpdateTimerText();
    }

    /// <summary>
    /// Checks if either fighter has been defeated
    /// </summary>
    private void CheckForRoundWinner()
    {
        if (playerHealth != null && playerHealth.IsDefeated)
        {
            EndRound(enemyCharacter);
            return;
        }

        if (enemyHealth != null && enemyHealth.IsDefeated)
        {
            EndRound(playerCharacter);
        }
    }

    /// <summary>
    /// Ends the round when time expires
    /// Higher health wins If health is equal, the round is a draw and gives no point
    /// </summary>
    private void EndRoundByTime()
    {
        if (playerHealth == null || enemyHealth == null)
            return;

        int playerCurrentHealth = playerHealth.GetCurrentHealth();
        int enemyCurrentHealth = enemyHealth.GetCurrentHealth();

        if (playerCurrentHealth > enemyCurrentHealth)
        {
            EndRound(playerCharacter);
        }
        else if (enemyCurrentHealth > playerCurrentHealth)
        {
            EndRound(enemyCharacter);
        }
        else
        {
            EndRoundAsDraw();
        }
    }

    /// <summary>
    /// Ends the round and gives a round win to the winner
    /// </summary>
    private void EndRound(FightCharacter roundWinner)
    {
        if (!roundActive)
            return;

        roundActive = false;
        SetFightersActive(false);

        if (roundWinner == playerCharacter)
        {
            playerRoundWins++;
            SetRoundMessage("Player Wins Round");
        }
        else if (roundWinner == enemyCharacter)
        {
            enemyRoundWins++;
            SetRoundMessage("Enemy Wins Round");
        }

        UpdateRoundWinText();

        if (CheckForGameWinner())
            return;

        currentRoundNumber++;
        ShowNextRoundPrompt();
    }

    /// <summary>
    /// Ends the round as a draw
    /// No fighter gets a round point
    /// </summary>
    private void EndRoundAsDraw()
    {
        if (!roundActive)
            return;

        roundActive = false;
        SetFightersActive(false);

        SetRoundMessage("Draw Round");

        currentRoundNumber++;
        ShowNextRoundPrompt();
    }

    /// <summary>
    /// Checks whether either fighter has won the full game
    /// </summary>
    private bool CheckForGameWinner()
    {
        if (playerRoundWins >= roundsNeededToWinGame)
        {
            EndGame("Player Wins Game");
            return true;
        }

        if (enemyRoundWins >= roundsNeededToWinGame)
        {
            EndGame("Enemy Wins Game");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ends the full match
    /// </summary>
    private void EndGame(string message)
    {
        gameOver = true;
        roundActive = false;
        waitingForNextRound = false;
        startingRound = false;

        SetFightersActive(false);
        SetNextRoundPromptVisible(false);
        SetRoundMessage(message);

        Debug.Log(message);

        if (returnToStartScreenAfterGameOver)
            Invoke(nameof(ShowStartScreen), roundRestartDelay);
    }

    /// <summary>
    /// Resets fighter health, position, and velocity
    /// </summary>
    private void ResetFighters()
    {
        if (playerHealth != null)
            playerHealth.ResetHealth();

        if (enemyHealth != null)
            enemyHealth.ResetHealth();

        ResetFighterPosition(playerCharacter, playerRigidbody, playerStartPoint);
        ResetFighterPosition(enemyCharacter, enemyRigidbody, enemyStartPoint);
    }

    /// <summary>
    /// Resets one fighter's position and velocity
    /// </summary>
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

    /// <summary>
    /// Enables or disables fighter control
    /// </summary>
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

    /// <summary>
    /// Updates all round UI
    /// </summary>
    private void UpdateAllUI()
    {
        UpdateTimerText();
        UpdateRoundWinText();
    }

    /// <summary>
    /// Updates the timer text
    /// </summary>
    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(currentRoundTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = minutes.ToString("0") + ":" + seconds.ToString("00");
    }

    /// <summary>
    /// Updates round win text
    /// </summary>
    private void UpdateRoundWinText()
    {
        if (playerRoundWinsText != null)
            playerRoundWinsText.text = "Player: " + playerRoundWins;

        if (enemyRoundWinsText != null)
            enemyRoundWinsText.text = "Enemy: " + enemyRoundWins;
    }

    /// <summary>
    /// Updates the round message text
    /// </summary>
    private void SetRoundMessage(string message)
    {
        if (roundMessageText == null)
            return;

        roundMessageText.text = message;
    }

    private float TickTimer(float timer)
    {
        if (timer <= 0f)
            return 0f;

        return timer - Time.deltaTime;
    }
}