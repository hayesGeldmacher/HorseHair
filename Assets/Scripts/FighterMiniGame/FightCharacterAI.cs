using UnityEngine;

/// <summary>
///  Opponent AI for the fighting game
/// Controls spacing, attacking, blocking, and jump attacks through FightCharacter input
/// </summary>
public class FightCharacterAI : MonoBehaviour
{
    private enum AIState
    {
        Approaching,
        Spacing,
        BackingAway,
        Blocking,
        JumpAttack
    }

    private enum LastAction
    {
        None,
        StandingPunch,
        CrouchingPunch,
        JumpingPunch,
        Block
    }

    [Header("References")]
    [Tooltip("FightCharacter component on this enemy")]
    [SerializeField] private FightCharacter fightCharacter;

    [Tooltip("Player transform the AI reacts to")]
    [SerializeField] private Transform player;

    [Header("Spacing")]
    [Tooltip("If farther than this, the AI walks toward the player")]
    [SerializeField] private float approachDistance = 3.5f;

    [Tooltip("Ideal distance the AI tries to stay near")]
    [SerializeField] private float idealDistance = 1.35f;

    [Tooltip("If closer than this, the AI backs away")]
    [SerializeField] private float tooCloseDistance = 0.85f;

    [Tooltip("Distance where the AI is allowed to attack")]
    [SerializeField] private float attackDistance = 1.75f;

    [Tooltip("How much distance error is allowed before the AI adjusts position")]
    [SerializeField] private float spacingBuffer = 0.15f;

    [Header("Decision Timing")]
    [Tooltip("Minimum time between AI decisions")]
    [SerializeField] private float decisionCooldown = 0.65f;

    [Tooltip("Small delay before the AI reacts Higher values make the AI easier")]
    [SerializeField] private float reactionDelay = 0.12f;

    [Tooltip("How long the AI holds block")]
    [SerializeField] private float blockHoldTime = 0.35f;

    [Tooltip("Delay between jumping and punching during a jump attack")]
    [SerializeField] private float jumpPunchDelay = 0.2f;

    [Header("Action Cooldowns")]
    [Tooltip("Cooldown after the AI blocks so it does not block repeatedly")]
    [SerializeField] private float blockCooldown = 1.5f;

    [Tooltip("Cooldown after the AI jump attacks so it does not jump repeatedly")]
    [SerializeField] private float jumpAttackCooldown = 1.75f;

    [Header("Behavior Weights")]
    [Tooltip("Higher value means standing punch is more likely")]
    [SerializeField] private int standingPunchWeight = 50;

    [Tooltip("Higher value means crouching punch is more likely")]
    [SerializeField] private int crouchingPunchWeight = 20;

    [Tooltip("Higher value means jumping punch is more likely")]
    [SerializeField] private int jumpingPunchWeight = 20;

    [Tooltip("Higher value means blocking is more likely")]
    [SerializeField] private int blockWeight = 10;

    [Header("Movement Feel")]
    [Tooltip("Prevents the AI from switching left/right movement too rapidly")]
    [SerializeField] private float movementCommitTime = 0.2f;

    private AIState currentState;
    private LastAction lastAction;

    private float decisionTimer;
    private float reactionTimer;
    private float blockTimer;
    private float jumpPunchTimer;
    private float blockCooldownTimer;
    private float jumpAttackCooldownTimer;
    private float movementCommitTimer;

    private Vector2 currentMoveInput;

    private bool pendingDecision;
    private bool waitingForJumpPunch;

    private void Reset()
    {
        AssignDefaultReferences();
    }

    private void Awake()
    {
        AssignMissingReferences();
    }

    private void Start()
    {
        currentState = AIState.Spacing;
        lastAction = LastAction.None;
    }

    private void Update()
    {
        if (fightCharacter == null || player == null)
            return;

        UpdateTimers();

        if (waitingForJumpPunch)
        {
            HandleJumpAttack();
            return;
        }

        if (currentState == AIState.Blocking)
        {
            HandleBlock();
            return;
        }

        if (pendingDecision)
        {
            HandleReactionDelay();
            return;
        }

        UpdateMovementState();

        if (CanMakeDecision())
        {
            StartDecisionDelay();
        }

        SendMovementOnlyInput();
    }

    /// <summary>
    /// Assigns component references automatically when the script is added or reset in the Inspector
    /// </summary>
    private void AssignDefaultReferences()
    {
        fightCharacter = GetComponent<FightCharacter>();
    }

    /// <summary>
    /// Assigns required references at runtime if they were not assigned in the Inspector
    /// </summary>
    private void AssignMissingReferences()
    {
        if (fightCharacter == null)
            fightCharacter = GetComponent<FightCharacter>();
    }

    /// <summary>
    /// Updates the AI's movement state based on distance from the player
    /// </summary>
    private void UpdateMovementState()
    {
        float distance = GetAbsDistanceToPlayer();

        if (distance > approachDistance)
        {
            currentState = AIState.Approaching;
            SetCommittedMove(GetDirectionTowardPlayer());
            return;
        }

        if (distance < tooCloseDistance)
        {
            currentState = AIState.BackingAway;
            SetCommittedMove(GetDirectionAwayFromPlayer());
            return;
        }

        if (distance > idealDistance + spacingBuffer)
        {
            currentState = AIState.Approaching;
            SetCommittedMove(GetDirectionTowardPlayer());
            return;
        }

        if (distance < idealDistance - spacingBuffer)
        {
            currentState = AIState.BackingAway;
            SetCommittedMove(GetDirectionAwayFromPlayer());
            return;
        }

        currentState = AIState.Spacing;
        SetCommittedMove(0f);
    }

    /// <summary>
    /// Starts a reaction delay before choosing an action
    /// </summary>
    private void StartDecisionDelay()
    {
        pendingDecision = true;
        reactionTimer = reactionDelay;
    }

    /// <summary>
    /// Waits for reaction delay, then chooses an action
    /// </summary>
    private void HandleReactionDelay()
    {
        SendMovementOnlyInput();

        if (reactionTimer > 0f)
            return;

        pendingDecision = false;
        decisionTimer = decisionCooldown;

        ChooseAction();
    }

    /// <summary>
    /// Chooses an action using weighted values
    /// Recent actions are slightly discouraged so the AI has more variety
    /// </summary>
    private void ChooseAction()
    {
        float distance = GetAbsDistanceToPlayer();

        if (distance > attackDistance)
            return;

        int standingWeight = standingPunchWeight;
        int crouchingWeight = crouchingPunchWeight;
        int jumpingWeight = jumpingPunchWeight;
        int blockingWeight = blockWeight;

        if (lastAction == LastAction.StandingPunch)
            standingWeight = Mathf.RoundToInt(standingWeight * 0.5f);

        if (lastAction == LastAction.CrouchingPunch)
            crouchingWeight = Mathf.RoundToInt(crouchingWeight * 0.35f);

        if (lastAction == LastAction.JumpingPunch)
            jumpingWeight = 0;

        if (lastAction == LastAction.Block)
            blockingWeight = 0;

        if (blockCooldownTimer > 0f)
            blockingWeight = 0;

        if (jumpAttackCooldownTimer > 0f)
            jumpingWeight = 0;

        int totalWeight = standingWeight + crouchingWeight + jumpingWeight + blockingWeight;

        if (totalWeight <= 0)
        {
            DoStandingPunch();
            return;
        }

        int roll = Random.Range(0, totalWeight);

        if (roll < standingWeight)
        {
            DoStandingPunch();
            return;
        }

        roll -= standingWeight;

        if (roll < crouchingWeight)
        {
            DoCrouchingPunch();
            return;
        }

        roll -= crouchingWeight;

        if (roll < jumpingWeight)
        {
            StartJumpAttack();
            return;
        }

        StartBlock();
    }

    /// <summary>
    /// Sends standing punch input
    /// </summary>
    private void DoStandingPunch()
    {
        lastAction = LastAction.StandingPunch;
        fightCharacter.SetAIInput(Vector2.zero, false, true);
    }

    /// <summary>
    /// Sends crouch plus punch input
    /// </summary>
    private void DoCrouchingPunch()
    {
        lastAction = LastAction.CrouchingPunch;
        fightCharacter.SetAIInput(new Vector2(0f, -1f), false, true);
    }

    /// <summary>
    /// Starts a jump, then waits briefly before punching in the air
    /// </summary>
    private void StartJumpAttack()
    {
        currentState = AIState.JumpAttack;
        lastAction = LastAction.JumpingPunch;

        waitingForJumpPunch = true;
        jumpPunchTimer = jumpPunchDelay;
        jumpAttackCooldownTimer = jumpAttackCooldown;

        fightCharacter.SetAIInput(Vector2.zero, true, false);
    }

    /// <summary>
    /// Sends punch input after the jump delay
    /// </summary>
    private void HandleJumpAttack()
    {
        if (jumpPunchTimer > 0f)
        {
            fightCharacter.SetAIInput(Vector2.zero, false, false);
            return;
        }

        waitingForJumpPunch = false;
        currentState = AIState.Spacing;

        fightCharacter.SetAIInput(Vector2.zero, false, true);
    }

    /// <summary>
    /// Starts a standing or crouching block
    /// </summary>
    private void StartBlock()
    {
        currentState = AIState.Blocking;
        lastAction = LastAction.Block;

        blockTimer = blockHoldTime;
        blockCooldownTimer = blockCooldown;

        bool crouchBlock = Random.value < 0.35f;

        float backDirection = GetDirectionAwayFromPlayer();
        currentMoveInput = new Vector2(backDirection, crouchBlock ? -1f : 0f);

        fightCharacter.SetAIInput(currentMoveInput, false, false);
    }

    /// <summary>
    /// Holds block until the block timer ends
    /// </summary>
    private void HandleBlock()
    {
        if (blockTimer <= 0f)
        {
            currentState = AIState.Spacing;
            currentMoveInput = Vector2.zero;
            fightCharacter.SetAIInput(Vector2.zero, false, false);
            return;
        }

        fightCharacter.SetAIInput(currentMoveInput, false, false);
    }

    /// <summary>
    /// Sends movement input without jump or punch
    /// </summary>
    private void SendMovementOnlyInput()
    {
        fightCharacter.SetAIInput(currentMoveInput, false, false);
    }

    /// <summary>
    /// Sets horizontal movement but prevents rapid direction flipping
    /// </summary>
    private void SetCommittedMove(float horizontal)
    {
        if (movementCommitTimer > 0f)
            return;

        currentMoveInput = new Vector2(horizontal, 0f);
        movementCommitTimer = movementCommitTime;
    }

    /// <summary>
    /// Returns true if the AI is allowed to choose a new action
    /// </summary>
    private bool CanMakeDecision()
    {
        if (decisionTimer > 0f)
            return false;

        if (currentState != AIState.Spacing)
            return false;

        return GetAbsDistanceToPlayer() <= attackDistance;
    }

    /// <summary>
    /// Returns the horizontal distance to the player
    /// </summary>
    private float GetAbsDistanceToPlayer()
    {
        return Mathf.Abs(player.position.x - transform.position.x);
    }

    /// <summary>
    /// Returns direction toward the player
    /// </summary>
    private float GetDirectionTowardPlayer()
    {
        return Mathf.Sign(player.position.x - transform.position.x);
    }

    /// <summary>
    /// Returns direction away from the player
    /// </summary>
    private float GetDirectionAwayFromPlayer()
    {
        return -Mathf.Sign(player.position.x - transform.position.x);
    }

    /// <summary>
    /// Updates AI timers
    /// </summary>
    private void UpdateTimers()
    {
        if (decisionTimer > 0f)
            decisionTimer -= Time.deltaTime;

        if (reactionTimer > 0f)
            reactionTimer -= Time.deltaTime;

        if (blockTimer > 0f)
            blockTimer -= Time.deltaTime;

        if (jumpPunchTimer > 0f)
            jumpPunchTimer -= Time.deltaTime;

        if (blockCooldownTimer > 0f)
            blockCooldownTimer -= Time.deltaTime;

        if (jumpAttackCooldownTimer > 0f)
            jumpAttackCooldownTimer -= Time.deltaTime;

        if (movementCommitTimer > 0f)
            movementCommitTimer -= Time.deltaTime;
    }
}