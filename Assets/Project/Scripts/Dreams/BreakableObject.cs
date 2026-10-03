using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An object that blocks the player until attacks reduce its health to zero.
/// </summary>
public class BreakableObject : MonoBehaviour
{
    [Header("Health")]
    [Min(1)]
    [SerializeField] private int maxHealth = 30;

    [Header("Animation")]
    [Tooltip("Animator containing the hit & break animations.")]
    [SerializeField] private Animator objectAnimator;

    [Tooltip("Animator trigger played when the object is damaged.")]
    [SerializeField] private string hitTriggerName = "hit";

    [Tooltip("Animator trigger played when the object breaks.")]
    [SerializeField] private string breakTriggerName = "break";

    [Header("Blocking")]
    [Tooltip(
        "Colliders disabled when the object breaks " +
        "Leave empty to find all child colliders automatically"
    )]
    [SerializeField] private Collider[] blockingColliders;

    [Header("Cleanup")]
    [Tooltip("Destroy the object after its break animation")]
    [SerializeField] private bool destroyAfterBreaking;

    [Min(0f)]
    [SerializeField] private float destroyDelay = 2f;

    private int currentHealth;
    private bool isBroken;

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public bool IsBroken
    {
        get { return isBroken; }
    }

    private void Reset()
    {
        AssignMissingReferences();
    }

    private void Awake()
    {
        AssignMissingReferences();
        currentHealth = maxHealth;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        destroyDelay = Mathf.Max(0f, destroyDelay);
    }

    private void AssignMissingReferences()
    {
        if (objectAnimator == null)
            objectAnimator = GetComponentInChildren<Animator>();

        if (blockingColliders == null || blockingColliders.Length == 0)
        {
            blockingColliders =
                GetComponentsInChildren<Collider>(true);
        }
    }

    /// <summary>
    /// Called by an attack when this object is hit.
    /// </summary>
    public bool TakeDamage(int damage)
    {
        if (isBroken || damage <= 0)
            return false;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
        {
            BreakObject();
            return true;
        }

        PlayTrigger(hitTriggerName);
        return true;
    }

    private void BreakObject()
    {
        if (isBroken)
            return;

        isBroken = true;

        PlayTrigger(breakTriggerName);

        DisableBlockingColliders();

        if (destroyAfterBreaking)
            Destroy(gameObject, destroyDelay);
    }

    private void DisableBlockingColliders()
    {
        if (blockingColliders == null)
            return;

        foreach (Collider blockingCollider in blockingColliders)
        {
            if (blockingCollider != null)
                blockingCollider.enabled = false;
        }
    }

    private void PlayTrigger(string triggerName)
    {
        if (objectAnimator == null)
            return;

        if (string.IsNullOrWhiteSpace(triggerName))
            return;

        objectAnimator.SetTrigger(triggerName);
    }
}