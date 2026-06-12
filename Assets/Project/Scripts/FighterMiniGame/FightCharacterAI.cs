using UnityEngine;

/// <summary>
/// Opponent AI for the fighting game
/// Controls spacing, attacking, blocking, jump attacks, quickstep, round-start easing, breathing space, and player pressure response through FightCharacter input
/// </summary>
public class FightCharacterAI : MonoBehaviour
{
    private enum AIState
    {
        Approaching,
        Spacing,
        BackingAway,
        Blocking,
        JumpAttack,
        Quickstep
    }

    private enum LastAction
    {
        None,
        StandingPunch,
        CrouchingPunch,
        JumpingPunch,
        StandingKick,
        CrouchingKick,
        JumpingKick,
        Block,
        Quickstep
    }

    private enum JumpAttackType
    {
        Punch,
        Kick
    }

    [Header("References")]
    [Tooltip("FightCharacter component on this enemy")]
    [SerializeField] private FightCharacter fightCharacter;

    [Tooltip("Player transform the AI reacts to")]
    [SerializeField] private Transform player;

    [Header("Round Start Behavior")]
    [Tooltip("How long the AI uses slower, more cautious movement after a round starts")]
    [SerializeField] private float openingEaseTime = 2f;

    [Tooltip("Movement strength during the opening ease period")]
    [SerializeField] private float openingApproachStrength = 0.35f;

    [Tooltip("Chance that the AI slowly approaches during the opening")]
    [SerializeField] private float openingSlowApproachChance = 0.6f;

    [Tooltip("Chance that the AI waits during the opening")]
    [SerializeField] private float openingWaitChance = 0.25f;

    [Tooltip("Chance that the AI briefly backs away during the opening")]
    [SerializeField] private float openingBackAwayChance = 0.15f;

    [Tooltip("Minimum time before the AI changes opening movement")]
    [SerializeField] private float openingChoiceMinTime = 0.35f;

    [Tooltip("Maximum time before the AI changes opening movement")]
    [SerializeField] private float openingChoiceMaxTime = 0.8f;

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
    [SerializeField] private float decisionCooldown = 0.75f;

    [Tooltip("Small delay before the AI reacts Higher values make the AI easier")]
    [SerializeField] private float reactionDelay = 0.12f;

    [Tooltip("How long the AI holds block")]
    [SerializeField] private float blockHoldTime = 0.35f;

    [Tooltip("Delay between jumping and attacking during a jump attack")]
    [SerializeField] private float jumpAttackDelay = 0.2f;

    [Header("Action Cooldowns")]
    [Tooltip("Cooldown after the AI blocks so it does not block repeatedly")]
    [SerializeField] private float blockCooldown = 1.5f;

    [Tooltip("Cooldown after the AI jump attacks so it does not jump repeatedly")]
    [SerializeField] private float jumpAttackCooldown = 2.5f;

    [Tooltip("Cooldown after the AI quicksteps so it does not quickstep repeatedly")]
    [SerializeField] private float aiQuickstepCooldown = 1.25f;

    [Header("Behavior Weights")]
    [Tooltip("Higher value means standing punch is more likely")]
    [SerializeField] private int standingPunchWeight = 50;

    [Tooltip("Higher value means crouching punch is more likely")]
    [SerializeField] private int crouchingPunchWeight = 25;

    [Tooltip("Higher value means jumping punch is more likely")]
    [SerializeField] private int jumpingPunchWeight = 10;

    [Tooltip("Higher value means standing kick is more likely")]
    [SerializeField] private int standingKickWeight = 30;

    [Tooltip("Higher value means crouching kick is more likely")]
    [SerializeField] private int crouchingKickWeight = 15;

    [Tooltip("Higher value means jumping kick is more likely")]
    [SerializeField] private int jumpingKickWeight = 10;

    [Tooltip("Higher value means blocking is more likely")]
    [SerializeField] private int blockWeight = 15;

    [Tooltip("Higher value means quickstep is more likely")]
    [SerializeField] private int quickstepWeight = 20;

    [Header("Quickstep")]
    [Tooltip("How long the AI holds the first quickstep tap")]
    [SerializeField] private float quickstepTapTime = 0.06f;

    [Tooltip("How long the AI releases movement between quickstep taps")]
    [SerializeField] private float quickstepReleaseTime = 0.06f;

    [Tooltip("How long the AI pauses after sending the quickstep input")]
    [SerializeField] private float quickstepAfterPauseTime = 0.2f;

    [Tooltip("Chance that the AI quicksteps away from the player instead of toward the player")]
    [SerializeField] private float quickstepAwayChance = 0.65f;

    [Header("Movement Feel")]
    [Tooltip("Prevents the AI from switching left/right movement too rapidly")]
    [SerializeField] private float movementCommitTime = 0.2f;

    [Header("Breathing Space")]
    [Tooltip("How many attacks the AI can do before it backs away")]
    [SerializeField] private int attacksBeforeBackAway = 2;

    [Tooltip("How long the AI backs away after attacking several times")]
    [SerializeField] private float forcedBackAwayTime = 0.65f;

    [Tooltip("How long the AI waits after backing away")]
    [SerializeField] private float recoveryPauseTime = 0.25f;

    [Tooltip("Distance where the AI is considered too close and should back away")]
    [SerializeField] private float emergencyBackAwayDistance = 0.65f;

    [Header("Player Pressure Response")]
    [Tooltip("If true, the AI backs up when the player walks toward it")]
    [SerializeField] private bool respectPlayerForwardPressure = true;

    [Tooltip("Distance where the AI starts respecting player forward movement")]
    [SerializeField] private float pressureRespectDistance = 2.25f;

    [Tooltip("How long the player must move forward before the AI reacts")]
    [SerializeField] private float pressureReactionTime = 0.15f;

    [Tooltip("How long the AI backs away after detecting player pressure")]
    [SerializeField] private float pressureBackAwayTime = 0.45f;

    [Tooltip("Minimum player movement speed needed to count as forward pressure")]
    [SerializeField] private float playerPressureSpeedThreshold = 0.05f;

    private AIState currentState;
    private LastAction lastAction;
    private JumpAttackType currentJumpAttackType;

    private float decisionTimer;
    private float reactionTimer;
    private float blockTimer;
    private float jumpAttackTimer;
    private float blockCooldownTimer;
    private float jumpAttackCooldownTimer;
    private float aiQuickstepCooldownTimer;
    private float movementCommitTimer;

    private float openingEaseTimer;
    private float openingChoiceTimer;

    private float forcedBackAwayTimer;
    private float recoveryPauseTimer;

    private float pressureTimer;
    private float pressureBackAwayTimer;
    private float previousPlayerX;

    private float quickstepInputTimer;
    private int quickstepPhase;
    private float quickstepInputDirection;

    private float distanceToPlayer;
    private float directionToPlayer;

    private int attackSequenceCount;

    private Vector2 currentMoveInput;
    private Vector2 openingMoveInput;

    private bool pendingDecision;
    private bool waitingForJumpAttack;
    private bool openingBehaviorActive;
    private bool wasRoundActive;

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
        ResetAIState();

        if (player != null)
            previousPlayerX = player.position.x;

        if (fightCharacter != null)
            wasRoundActive = fightCharacter.IsRoundActive;
    }

    private void Update()
    {
        if (fightCharacter == null || player == null)
            return;

        UpdateSpatialData();
        UpdateRoundActiveState();

        if (!fightCharacter.IsRoundActive)
        {
            SendIdleInput();
            return;
        }

        UpdateTimers();

        if (HandleOpeningBehavior())
            return;

        if (currentState == AIState.Quickstep)
        {
            HandleQuickstep();
            return;
        }

        if (HandlePlayerForwardPressure())
            return;

        if (HandleBreathingSpace())
            return;

        if (waitingForJumpAttack)
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
        AssignMissingReferences();
    }

    /// <summary>
    /// Assigns required references at runtime if they were not assigned in the Inspector
    /// </summary>
    private void AssignMissingReferences()
    {
        fightCharacter ??= GetComponent<FightCharacter>();
    }

    /// <summary>
    /// Detects when a new round starts and begins slower opening behavior
    /// </summary>
    private void UpdateRoundActiveState()
    {
        bool isRoundActive = fightCharacter.IsRoundActive;

        if (isRoundActive && !wasRoundActive)
        {
            ResetAIState();
            StartOpeningBehavior();
        }

        wasRoundActive = isRoundActive;
    }

    private void UpdateSpatialData()
    {
        float deltaX = player.position.x - transform.position.x;

        distanceToPlayer = Mathf.Abs(deltaX);
        directionToPlayer = Mathf.Sign(deltaX);
    }

    /// <summary>
    /// Resets AI timers and temporary actions
    /// </summary>
    private void ResetAIState()
    {
        currentState = AIState.Spacing;
        lastAction = LastAction.None;
        currentJumpAttackType = JumpAttackType.Punch;

        decisionTimer = 0f;
        reactionTimer = 0f;
        blockTimer = 0f;
        jumpAttackTimer = 0f;
        blockCooldownTimer = 0f;
        jumpAttackCooldownTimer = 0f;
        aiQuickstepCooldownTimer = 0f;
        movementCommitTimer = 0f;

        openingEaseTimer = 0f;
        openingChoiceTimer = 0f;

        forcedBackAwayTimer = 0f;
        recoveryPauseTimer = 0f;

        pressureTimer = 0f;
        pressureBackAwayTimer = 0f;

        quickstepInputTimer = 0f;
        quickstepPhase = 0;
        quickstepInputDirection = 0f;

        distanceToPlayer = 0f;
        directionToPlayer = 0f;

        attackSequenceCount = 0;

        currentMoveInput = Vector2.zero;
        openingMoveInput = Vector2.zero;

        pendingDecision = false;
        waitingForJumpAttack = false;
        openingBehaviorActive = false;

        if (player != null)
            previousPlayerX = player.position.x;
    }

    /// <summary>
    /// Starts the AI's slower opening behavior
    /// </summary>
    private void StartOpeningBehavior()
    {
        openingBehaviorActive = true;
        openingEaseTimer = openingEaseTime;
        openingChoiceTimer = 0f;

        PickOpeningMovement();
    }

    /// <summary>
    /// Chooses whether the AI slowly approaches, waits, or backs away during the round opening
    /// </summary>
    private void PickOpeningMovement()
    {
        float totalChance = openingSlowApproachChance + openingWaitChance + openingBackAwayChance;

        if (totalChance <= 0f)
        {
            openingMoveInput = Vector2.zero;
            return;
        }

        float roll = Random.Range(0f, totalChance);

        if (roll < openingSlowApproachChance)
        {
            openingMoveInput = new Vector2(GetDirectionTowardPlayer() * openingApproachStrength, 0f);
        }
        else if (roll < openingSlowApproachChance + openingWaitChance)
        {
            openingMoveInput = Vector2.zero;
        }
        else
        {
            openingMoveInput = new Vector2(GetDirectionAwayFromPlayer() * openingApproachStrength, 0f);
        }

        openingChoiceTimer = Random.Range(openingChoiceMinTime, openingChoiceMaxTime);
    }

    /// <summary>
    /// Handles slower, less aggressive AI movement at the beginning of a round
    /// </summary>
    private bool HandleOpeningBehavior()
    {
        if (!openingBehaviorActive)
            return false;

        openingEaseTimer -= Time.deltaTime;
        openingChoiceTimer -= Time.deltaTime;

        if (openingEaseTimer <= 0f)
        {
            openingBehaviorActive = false;
            currentMoveInput = Vector2.zero;
            return false;
        }

        if (openingChoiceTimer <= 0f)
        {
            PickOpeningMovement();
        }

        SendMovementInput(openingMoveInput);
        return true;
    }

    /// <summary>
    /// Makes the AI give ground when the player walks forward into close range
    /// </summary>
    private bool HandlePlayerForwardPressure()
    {
        if (!respectPlayerForwardPressure)
        {
            previousPlayerX = player.position.x;
            return false;
        }

        if (pressureBackAwayTimer > 0f)
        {
            previousPlayerX = player.position.x;
            SendMovementInput(new Vector2(GetDirectionAwayFromPlayer(), 0f));
            return true;
        }

        float playerDeltaX = player.position.x - previousPlayerX;
        previousPlayerX = player.position.x;

        float directionFromPlayerToAI = Mathf.Sign(transform.position.x - player.position.x);
        bool playerMovingTowardAI = Mathf.Sign(playerDeltaX) == directionFromPlayerToAI;
        bool playerMovedEnough = Mathf.Abs(playerDeltaX) > playerPressureSpeedThreshold * Time.deltaTime;
        bool closeEnough = GetAbsDistanceToPlayer() <= pressureRespectDistance;

        if (playerMovingTowardAI && playerMovedEnough && closeEnough)
        {
            pressureTimer += Time.deltaTime;

            if (pressureTimer >= pressureReactionTime)
            {
                pressureTimer = 0f;
                pressureBackAwayTimer = pressureBackAwayTime;
                attackSequenceCount = 0;
                return true;
            }
        }
        else
        {
            pressureTimer = 0f;
        }

        return false;
    }

    /// <summary>
    /// Gives the AI breathing space by backing away after pressure or when too close
    /// </summary>
    private bool HandleBreathingSpace()
    {
        if (recoveryPauseTimer > 0f)
        {
            SendIdleInput();
            return true;
        }

        if (forcedBackAwayTimer > 0f)
        {
            SendMovementInput(new Vector2(GetDirectionAwayFromPlayer(), 0f));
            return true;
        }

        if (GetAbsDistanceToPlayer() < emergencyBackAwayDistance)
        {
            forcedBackAwayTimer = forcedBackAwayTime;
            attackSequenceCount = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tracks AI pressure and forces a back away after several attacks
    /// </summary>
    private void RegisterAttack()
    {
        attackSequenceCount++;

        if (attackSequenceCount >= attacksBeforeBackAway)
        {
            attackSequenceCount = 0;
            forcedBackAwayTimer = forcedBackAwayTime;
        }
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
        if (GetAbsDistanceToPlayer() > attackDistance)
            return;

        int standingPunch = standingPunchWeight;
        int crouchingPunch = crouchingPunchWeight;
        int jumpingPunch = jumpingPunchWeight;
        int standingKick = standingKickWeight;
        int crouchingKick = crouchingKickWeight;
        int jumpingKick = jumpingKickWeight;
        int blockingWeight = blockWeight;
        int quickstep = quickstepWeight;

        if (lastAction == LastAction.StandingPunch)
            standingPunch = ReduceWeight(standingPunch, 0.75f);

        if (lastAction == LastAction.CrouchingPunch)
            crouchingPunch = ReduceWeight(crouchingPunch, 0.75f);

        if (lastAction == LastAction.JumpingPunch)
            jumpingPunch = 0;

        if (lastAction == LastAction.StandingKick)
            standingKick = ReduceWeight(standingKick, 0.75f);

        if (lastAction == LastAction.CrouchingKick)
            crouchingKick = ReduceWeight(crouchingKick, 0.75f);

        if (lastAction == LastAction.JumpingKick)
            jumpingKick = 0;

        if (lastAction == LastAction.Block)
            blockingWeight = 0;

        if (lastAction == LastAction.Quickstep)
            quickstep = 0;

        if (blockCooldownTimer > 0f)
            blockingWeight = 0;

        if (jumpAttackCooldownTimer > 0f)
        {
            jumpingPunch = 0;
            jumpingKick = 0;
        }

        if (aiQuickstepCooldownTimer > 0f)
            quickstep = 0;

        int totalWeight =
            standingPunch +
            crouchingPunch +
            jumpingPunch +
            standingKick +
            crouchingKick +
            jumpingKick +
            blockingWeight +
            quickstep;

        if (totalWeight <= 0)
            return;

        int roll = Random.Range(0, totalWeight);

        if (TryUseWeightedAction(ref roll, standingPunch, DoStandingPunch))
            return;

        if (TryUseWeightedAction(ref roll, crouchingPunch, DoCrouchingPunch))
            return;

        if (TryUseWeightedAction(ref roll, jumpingPunch, StartJumpPunch))
            return;

        if (TryUseWeightedAction(ref roll, standingKick, DoStandingKick))
            return;

        if (TryUseWeightedAction(ref roll, crouchingKick, DoCrouchingKick))
            return;

        if (TryUseWeightedAction(ref roll, jumpingKick, StartJumpKick))
            return;

        if (TryUseWeightedAction(ref roll, blockingWeight, StartBlock))
            return;

        StartQuickstep();
    }

    private bool TryUseWeightedAction(ref int roll, int weight, System.Action action)
    {
        if (weight <= 0)
            return false;

        if (roll < weight)
        {
            action.Invoke();
            return true;
        }

        roll -= weight;
        return false;
    }

    private int ReduceWeight(int weight, float multiplier)
    {
        return Mathf.RoundToInt(weight * multiplier);
    }

    /// <summary>
    /// Sends standing punch input
    /// </summary>
    private void DoStandingPunch()
    {
        lastAction = LastAction.StandingPunch;
        fightCharacter.SetAIInput(0f, false, true, false, false);
        RegisterAttack();
    }

    /// <summary>
    /// Sends crouch plus punch input
    /// </summary>
    private void DoCrouchingPunch()
    {
        lastAction = LastAction.CrouchingPunch;
        fightCharacter.SetAIInput(0f, false, true, false, true);

        RegisterAttack();
    }

    /// <summary>
    /// Starts a jump, then waits briefly before punching in the air
    /// </summary>
    private void StartJumpPunch()
    {
        currentState = AIState.JumpAttack;
        lastAction = LastAction.JumpingPunch;
        currentJumpAttackType = JumpAttackType.Punch;

        waitingForJumpAttack = true;
        jumpAttackTimer = jumpAttackDelay;
        jumpAttackCooldownTimer = jumpAttackCooldown;

        fightCharacter.SetAIInput(0f, true, false, false, false);

        RegisterAttack();
    }

    /// <summary>
    /// Sends punch or kick input after the jump delay
    /// </summary>
    private void HandleJumpAttack()
    {
        if (jumpAttackTimer > 0f)
        {
            SendIdleInput();
            return;
        }

        waitingForJumpAttack = false;
        currentState = AIState.Spacing;

        if (currentJumpAttackType == JumpAttackType.Kick)
        {
            fightCharacter.SetAIInput(0f, false, false, true, false);
        }
        else
        {
            fightCharacter.SetAIInput(0f, false, true, false, false);
        }
    }

    /// <summary>
    /// Sends standing kick input
    /// </summary>
    private void DoStandingKick()
    {
        lastAction = LastAction.StandingKick;
        fightCharacter.SetAIInput(0f, false, false, true, false);
        RegisterAttack();
    }

    /// <summary>
    ///  Sends crouch plus kick input
    /// </summary>
    private void DoCrouchingKick()
    {
        lastAction = LastAction.CrouchingKick;
        fightCharacter.SetAIInput(0f, false, false, true, true);
        RegisterAttack();
    }

    /// <summary>
    /// Starts a jump, then waits briefly before kicking in the air
    /// </summary>
    private void StartJumpKick()
    {
        currentState = AIState.JumpAttack;
        lastAction = LastAction.JumpingKick;
        currentJumpAttackType = JumpAttackType.Kick;

        waitingForJumpAttack = true;
        jumpAttackTimer = jumpAttackDelay;
        jumpAttackCooldownTimer = jumpAttackCooldown;

        fightCharacter.SetAIInput(0f, true, false, false, false);

        RegisterAttack();
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

        SendMovementOnlyInput();
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
            SendIdleInput();
            return;
        }

        SendMovementOnlyInput();
    }

    /// <summary>
    /// Starts a quickstep by sending double-tap movement input to FightCharacter
    /// </summary>
    private void StartQuickstep()
    {
        currentState = AIState.Quickstep;
        lastAction = LastAction.Quickstep;

        aiQuickstepCooldownTimer = aiQuickstepCooldown;

        bool stepAway = Random.value < quickstepAwayChance;
        quickstepInputDirection = stepAway ? GetDirectionAwayFromPlayer() : GetDirectionTowardPlayer();

        quickstepPhase = 0;
        quickstepInputTimer = quickstepTapTime;

        attackSequenceCount = 0;

        fightCharacter.SetAIInput(quickstepInputDirection, false, false, false, false);
    }

    /// <summary>
    /// Sends the tap, release, tap sequence needed to trigger FightCharacter quickstep
    /// </summary>
    private void HandleQuickstep()
    {
        quickstepInputTimer -= Time.deltaTime;

        if (quickstepInputTimer > 0f)
            return;

        quickstepPhase++;

        switch (quickstepPhase)
        {
            case 1:
                quickstepInputTimer = quickstepReleaseTime;
                fightCharacter.SetAIInput(0f, false, false, false, false);
                break;

            case 2:
                quickstepInputTimer = quickstepTapTime;
                fightCharacter.SetAIInput(quickstepInputDirection, false, false, false, false);
                break;

            case 3:
                quickstepInputTimer = quickstepAfterPauseTime;
                fightCharacter.SetAIInput(0f, false, false, false, false);
                break;

            default:
                currentState = AIState.Spacing;
                currentMoveInput = Vector2.zero;
                SendIdleInput();
                break;
        }
    }

    /// <summary>
    /// Sends movement input without jump or punch
    /// </summary>
    private void SendMovementOnlyInput()
    {
        SendMovementInput(currentMoveInput);
    }

    private void SendIdleInput()
    {
        SendMovementInput(Vector2.zero);
    }

    private void SendMovementInput(Vector2 movement)
    {
        fightCharacter.SetAIInput(movement.x, false, false, false, movement.y < -0.25f);
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
        return decisionTimer <= 0f
            && currentState == AIState.Spacing
            && GetAbsDistanceToPlayer() <= attackDistance;
    }

    /// <summary>
    /// Returns the horizontal distance to the player
    /// </summary>
    private float GetAbsDistanceToPlayer()
    {
        return distanceToPlayer;
    }

    /// <summary>
    /// Returns direction toward the player
    /// </summary>
    private float GetDirectionTowardPlayer()
    {
        return directionToPlayer;
    }

    /// <summary>
    /// Returns direction away from the player
    /// </summary>
    private float GetDirectionAwayFromPlayer()
    {
        return -directionToPlayer;
    }

    /// <summary>
    /// Updates AI timers
    /// </summary>
    private void UpdateTimers()
    {
        decisionTimer = TickTimer(decisionTimer);
        reactionTimer = TickTimer(reactionTimer);
        blockTimer = TickTimer(blockTimer);
        jumpAttackTimer = TickTimer(jumpAttackTimer);
        blockCooldownTimer = TickTimer(blockCooldownTimer);
        jumpAttackCooldownTimer = TickTimer(jumpAttackCooldownTimer);
        aiQuickstepCooldownTimer = TickTimer(aiQuickstepCooldownTimer);
        movementCommitTimer = TickTimer(movementCommitTimer);
        pressureBackAwayTimer = TickTimer(pressureBackAwayTimer);

        if (forcedBackAwayTimer > 0f)
        {
            forcedBackAwayTimer = TickTimer(forcedBackAwayTimer);

            if (forcedBackAwayTimer <= 0f)
            {
                recoveryPauseTimer = recoveryPauseTime;
            }
        }

        recoveryPauseTimer = TickTimer(recoveryPauseTimer);
    }

    private float TickTimer(float timer)
    {
        if (timer <= 0f)
            return 0f;

        return timer - Time.deltaTime;
    }
}