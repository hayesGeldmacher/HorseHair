using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles fighter health, health bar UI, and defeat state
/// </summary>
public class FighterHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum health for this fighter")]
    [SerializeField] private int maxHealth = 100;

    [Header("UI")]
    [Tooltip("Slider used as this fighter's health bar")]
    [SerializeField] private Slider healthBar;

    private int currentHealth;
    private bool isDefeated;

    public bool IsDefeated
    {
        get { return isDefeated; }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        SetupHealthBar();
    }

    private void Start()
    {
        Debug.Log(name + " starting health: " + currentHealth);
    }

    /// <summary>
    /// Applies damage to this fighter
    /// If canDefeat is false, damage cannot reduce health below 1
    /// This is useful for chip damage
    /// </summary>
    public void TakeDamage(int damage, bool canDefeat = true)
    {
        if (isDefeated)
            return;

        int newHealth = currentHealth - damage;

        if (!canDefeat && newHealth <= 0)
        {
            newHealth = 1;
        }

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        UpdateHealthBar();

        Debug.Log(name + " took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Defeat();
        }
    }

    /// <summary>
    /// Returns the fighter's current health
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Returns the fighter's max health
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Sets up the health bar at full health
    /// </summary>
    private void SetupHealthBar()
    {
        if (healthBar == null)
        {
            Debug.LogWarning(name + " has no health bar assigned.");
            return;
        }

        healthBar.minValue = 0;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthBar.wholeNumbers = true;
        healthBar.interactable = false;
    }

    /// <summary>
    /// Updates the slider to match current health
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHealth;
    }

    /// <summary>
    /// Marks the fighter as defeated
    /// </summary>
    private void Defeat()
    {
        isDefeated = true;
        Debug.Log(name + " has been defeated.");
    }

    /// <summary>
    /// Resets this fighter back to full health
    /// Used when a new round starts
    /// </summary>
    public void ResetHealth()
    {
        isDefeated = false;
        currentHealth = maxHealth;
        UpdateHealthBar();

        Debug.Log(name + " health reset to " + currentHealth);
    }
}