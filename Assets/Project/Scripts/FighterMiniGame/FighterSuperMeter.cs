using UnityEngine;

/// <summary>
/// Handles the fighter's super meter and limits special ability usage per round.
/// </summary>
public class FighterSuperMeter : MonoBehaviour
{
    [Header("Special Uses")]
    [Tooltip("Maximum number of times this fighter can use special per round")]
    [Min(1)]
    [SerializeField] private int maxSpecialUses = 3;

    [Header("UI")]
    [Tooltip("Angled fill graphic used as this fighter's special meter")]
    [SerializeField] private AngledHealthBarFill superBar;

    private int currentSpecialUses;

    private void Awake()
    {
        ResetSuper();
    }

    public bool CanUseSpecial()
    {
        return currentSpecialUses > 0;
    }

    public bool TrySpendSpecial()
    {
        if (!CanUseSpecial())
            return false;

        currentSpecialUses--;

        UpdateSuperBar();

        Debug.Log(
            name + " used special. Uses left: " +
            currentSpecialUses
        );

        return true;
    }

    public void ResetSuper()
    {
        currentSpecialUses = maxSpecialUses;
        UpdateSuperBar();

        Debug.Log(
            name + " special uses reset to " +
            currentSpecialUses
        );
    }

    public int GetCurrentSpecialUses()
    {
        return currentSpecialUses;
    }

    public int GetMaxSpecialUses()
    {
        return maxSpecialUses;
    }

    private void UpdateSuperBar()
    {
        if (superBar == null)
        {
            Debug.LogWarning(
                name + " has no angled super bar assigned."
            );

            return;
        }

        float normalizedSpecial =
            (float)currentSpecialUses / maxSpecialUses;

        superBar.SetFill(normalizedSpecial);
    }

    [ContextMenu("Test: Spend Special")]
    private void TestSpendSpecial()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Enter Play Mode before testing the special bar."
            );

            return;
        }

        TrySpendSpecial();
    }

    [ContextMenu("Test: Reset Special")]
    private void TestResetSpecial()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Enter Play Mode before testing the special bar."
            );

            return;
        }

        ResetSuper();
    }
}