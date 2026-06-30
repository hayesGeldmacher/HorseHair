using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the fighter's super meter and limits special ability usage per round.
/// </summary>
public class FighterSuperMeter : MonoBehaviour
{
    [Header("Special Uses")]
    [Tooltip("Maximum number of times this fighter can use special per round")]
    [SerializeField] private int maxSpecialUses = 3;

    [Header("UI")]
    [Tooltip("Slider used as this fighter's special meter")]
    [SerializeField] private Slider superBar;

    private int currentSpecialUses;

    private void Awake()
    {
        ResetSuper();
        SetupSuperBar();
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

        Debug.Log(name + " used special. Uses left: " + currentSpecialUses);

        return true;
    }

    public void ResetSuper()
    {
        currentSpecialUses = maxSpecialUses;
        UpdateSuperBar();
    }

    public int GetCurrentSpecialUses()
    {
        return currentSpecialUses;
    }

    public int GetMaxSpecialUses()
    {
        return maxSpecialUses;
    }

    private void SetupSuperBar()
    {
        if (superBar == null)
        {
            Debug.LogWarning(name + " has no super bar assigned.");
            return;
        }

        superBar.minValue = 0;
        superBar.maxValue = maxSpecialUses;
        superBar.value = currentSpecialUses;
        superBar.wholeNumbers = true;
        superBar.interactable = false;
    }

    private void UpdateSuperBar()
    {
        if (superBar == null)
            return;

        superBar.value = currentSpecialUses;
    }
}