using UnityEngine;
using TMPro;

/// <summary>
/// Handles round timing, round wins, bo3 game rules, and round resets
/// </summary>
public class FightRoundManager : MonoBehaviour
{
    [Header("Round Rules")]
    [Tooltip("Length of each round in seconds 120 seconds = 2 minutes")]
    [SerializeField] private float roundTimeSeconds = 120f;

    [Tooltip("Number of round wins needed to win the full game")]
    [SerializeField] private int roundsNeededToWinGame = 2;

    [Tooltip("Delay before starting the next round after a round ends")]
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

    private bool roundActive;
    private bool gameOver;

    private void Start()
    {
        StartRound();
    }

    private void Update()
    {
        if (!roundActive || gameOver)
            return;

        UpdateRoundTimer();
        CheckForRoundWinner();
    }

    /// <summary>
    /// Starts or restarts a round
    /// </summary>
    private void StartRound()
    {
        currentRoundTime = roundTimeSeconds;
        roundActive = true;

        ResetFighters();
        SetFightersActive(true);
        UpdateAllUI();

        SetRoundMessage("Round " + currentRoundNumber);

        Debug.Log("Round " + currentRoundNumber + " started.");
    }

    /// <summary>
    /// Updates the round timer and ends the round when time reaches zero
    /// </summary>
    private void UpdateRoundTimer()
    {
        currentRoundTime -= Time.deltaTime;

        if (currentRoundTime <= 0f)
        {
            currentRoundTime = 0f;
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
            return;
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
        Invoke(nameof(StartRound), roundRestartDelay);
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
        Invoke(nameof(StartRound), roundRestartDelay);
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

        SetFightersActive(false);
        SetRoundMessage(message);

        Debug.Log(message);
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

        fighter.transform.position = startPoint.position;
        fighter.transform.rotation = startPoint.rotation;

        if (fighterRigidbody == null)
            return;

        fighterRigidbody.linearVelocity = Vector3.zero;
        fighterRigidbody.angularVelocity = Vector3.zero;
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
}