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
    [SerializeField] private AngledHealthFill healthBar;

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

    public void TakeDamage(int damage, bool canDefeat = true)
    {
        if (isDefeated)
            return;

        int newHealth = currentHealth - damage;

        if (!canDefeat && newHealth <= 0)
            newHealth = 1;

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        UpdateHealthBar();

        Debug.Log(name + " took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
            Defeat();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void SetupHealthBar()
    {
        if (healthBar == null)
        {
            Debug.LogWarning(name + " has no health bar assigned.");
            return;
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        float normalizedHealth = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

        healthBar.SetFill(normalizedHealth);
    }

    private void Defeat()
    {
        isDefeated = true;
        Debug.Log(name + " has been defeated.");
    }

    public void ResetHealth()
    {
        isDefeated = false;
        currentHealth = maxHealth;
        UpdateHealthBar();

        Debug.Log(name + " health reset to " + currentHealth);
    }
}