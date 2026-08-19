using UnityEngine;

/// <summary>
/// Controls fighter movement, combat actions, defensive states, AI input, and action text display
/// This script is used by both the player and the enemy
/// </summary>
public class FightCharacter : MonoBehaviour
{
    #region Inspector References

    [Header("References")]
    [Tooltip("Input component used by the player Leave empty for AI fighters")]
    [SerializeField] private FighterInput input;

    [Tooltip("Opponent this fighter faces and attacks")]
    [SerializeField] private Transform opponent;

    [Tooltip("Rigidbody used for movement and jumping")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("Health component used when this fighter takes damage")]
    [SerializeField] private FighterHealth health;

    [Tooltip("Fight camera that shakes when this fighter lands an attack")]
    [SerializeField] private FightCameraFollow fightCamera;

    [Header("Control Type")]
    [Tooltip("Turn this on for AI-controlled fighters Leave it off for the player")]
    [SerializeField] private bool controlledByAI;

    [Header("Dream Sequence")]
    [Tooltip("Only check this for dream sequences, disables combat")]
    [SerializeField] private bool dreamTraversalMode;

    #endregion

    #region Movement Settings

    [Header("Movement")]
    [Tooltip("Horizontal movement speed")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Horizontal movement speed while holding back to block")]
    [SerializeField] private float blockMoveSpeed = 2.5f;

    [Tooltip("Horizontal movement speed while crouching")]
    [SerializeField] private float crouchMoveSpeed = 2.5f;

    [Tooltip("Upward force applied when jumping")]
    [SerializeField] private float jumpForce = 7f;

    [Tooltip("Gravity multiplier while rising")]
    [Min(1f)]
    [SerializeField] private float risingGravityMultiplier = 2f;

    [Tooltip("Gravity multiplier while falling")]
    [Min(1f)]
    [SerializeField] private float fallingGravityMultiplier = 3.5f;

    [Tooltip("Minimum input value required before movement input is counted")]
    [SerializeField] private float inputDeadZone = 0.25f;

    [Header("Quickstep")]
    [Tooltip("Horizontal speed applied during a quickstep dodge")]
    [SerializeField] private float quickstepSpeed = 12f;

    [Tooltip("Duration of the quickstep dodge movement")]
    [SerializeField] private float quickstepTime = 0.15f;

    [Tooltip("Cooldown time after performing a quickstep before it can be used again")]
    [SerializeField] private float quickstepCooldown = 0.4f;

    [Tooltip("Maximum time between two taps in the same direction to trigger a quickstep")]
    [SerializeField] private float doubleTapWindow = 0.25f;

    [Header("Ground Check")]
    [Tooltip("Layer used to detect the ground")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Distance used by the downward ground check raycast")]
    [SerializeField] private float groundCheckDistance = 0.6f;

    #endregion

    #region Combat Settings

    [Header("Combat")]
    [Header("Punch Attacks")]
    [Tooltip("Damage dealt by a successful standing punch")]
    [SerializeField] private int standingPunchDamage = 10;

    [Tooltip("Maximum distance required for a standing punch to hit")]
    [SerializeField] private float standingPunchRange = 1.5f;

    [Tooltip("Damage dealt by a successful crouching punch")]
    [SerializeField] private int crouchingPunchDamage = 8;

    [Tooltip("Maximum distance required for a crouching punch to hit")]
    [SerializeField] private float crouchingPunchRange = 1.2f;

    [Tooltip("Damage dealt by a successful jumping punch")]
    [SerializeField] private int jumpingPunchDamage = 12;

    [Tooltip("Maximum distance required for a jumping punch to hit")]
    [SerializeField] private float jumpingPunchRange = 1.6f;

    [Header("Kick Attacks")]
    [Tooltip("Damage dealt by a successful standing kick")]
    [SerializeField] private int standingKickDamage = 16;

    [Tooltip("Maximum distance required for a standing kick to hit")]
    [SerializeField] private float standingKickRange = 2f;

    [Tooltip("Damage dealt by a successful crouching kick")]
    [SerializeField] private int crouchingKickDamage = 14;

    [Tooltip("Maximum distance required for a crouching kick to hit")]
    [SerializeField] private float crouchingKickRange = 1.8f;

    [Tooltip("Damage dealt by a successful jumping kick")]
    [SerializeField] private int jumpingKickDamage = 18;

    [Tooltip("Maximum distance required for a jumping kick to hit")]
    [SerializeField] private float jumpingKickRange = 2.1f;

    [Header("Grab")]
    [Tooltip("Damage dealt by a successful grab")]
    [SerializeField] private int grabDamage = 6;

    [Tooltip("Maximum distance required for a grab to hit")]
    [SerializeField] private float grabRange = 2f;

    [Tooltip("Distance between fighters after a successful grab side switch")]
    [SerializeField] private float grabSideSwitchOffset = 1f;

    [Tooltip("How long a grabbed fighter stays grounded before automatically recovering")]
    [SerializeField] private float groundedTime = 3f;

    [Tooltip("How long the fighter spends recovering after being grounded")]
    [SerializeField] private float recoveryTime = 0.6f;

    [Tooltip("How long recovery takes when attack is pressed")]
    [SerializeField] private float manualRecoveryTime = 1.2f;

    [Tooltip("Animation speed used for automatic recovery")]
    [SerializeField] private float automaticRecoveryAnimationSpeed = 1f;

    [Tooltip("Animation speed used for manual recovery")]
    [SerializeField] private float manualRecoveryAnimationSpeed = 0.65f;

    [Header("Super / Special")]
    [Tooltip("Reference to the super meter component used to track special ability usage")]
    [SerializeField] private FighterSuperMeter superMeter;

    [Tooltip("Damage dealt by a successful special attack")]
    [SerializeField] private int specialDamage = 30;

    [Tooltip("Maximum distance required for a special attack to hit")]
    [SerializeField] private float specialRange = 2.5f;

    [Tooltip("Sound played when this fighter performs a special attack")]
    [SerializeField] private AudioClip specialHitSound;

    [Header("Blocking")]
    [Tooltip("Damage dealt when an attack is blocked Chip damage cannot defeat")]
    [SerializeField] private int blockChipDamage = 2;

    [Tooltip("How long the blocking fighter is stunned after successfully blocking an attack")]
    [SerializeField] private float blockStunTime = 0.35f;

    [Tooltip("Small knockback applied after blocking an attack")]
    [SerializeField] private float blockKnockbackForce = 2f;

    [Tooltip("Pushback applied to the attacker when their attack is blocked")]
    [SerializeField] private float attackerBlockRecoilForce = 4f;

    [Header("Hit Pushback")]
    [Tooltip("Force applied to the opponent after a normal attack lands")]
    [SerializeField] private float hitPushbackForce = 3f;

    [Header("Hit Camera Shake")]
    [Tooltip("How long the camera shakes after this fighter lands an attack")]
    [Min(0f)]
    [SerializeField] private float hitShakeDuration = 0.08f;

    [Tooltip("How far the camera moves during a connected attack shake")]
    [Min(0f)]
    [SerializeField] private float hitShakeStrength = 0.05f;

    [Header("Fighter Pushboxes")]
    [Tooltip("Main body size while standing or crouching, increase width if idle animations allow fighters to overlap")]
    [SerializeField] private Vector2 standingPushboxSize = new Vector2(1.45f, 1.8f);

    [Tooltip("Body size while airborne, a shorter height lets fighters jump past each other more naturally")]
    [SerializeField] private Vector2 jumpingPushboxSize = new Vector2(0.8f, 1.25f);

    [Tooltip("Ground contact area while knocked down, increase width to stop opponents from stepping too close.")]
    [SerializeField] private Vector2 groundedPushboxSize = new Vector2(2.5f, 0.45f);

    [Tooltip("Body size while getting back up after a knockdown")]
    [SerializeField] private Vector2 recoveringPushboxSize = new Vector2(1.6f, 1.2f);

    [Tooltip("Extra spacing kept between fighters to prevent visual jitter when they touch")]
    [Min(0f)]
    [SerializeField] private float pushboxSeparationBuffer = 0.05f;

    [Header("Hitstop")]
    [Tooltip("How long the attacker freezes after landing a hit")]
    [SerializeField] private float attackerHitstopTime = 0.025f;

    [Tooltip("How long the defender freezes after being hit")]
    [SerializeField] private float defenderHitstopTime = 0.04f;

    [Tooltip("How long both fighters freeze when an attack is blocked")]
    [SerializeField] private float blockedHitstopTime = 0.02f;

    [Tooltip("How long player inputs are remembered during hitstop so they execute immediately afterward")]
    [SerializeField] private float hitstopInputBufferTime = 0.12f;

    private bool isInHitstop;
    private float hitstopTimer;
    private float animatorSpeedBeforeHitstop = 1f;

    private bool bufferedPunch;
    private bool bufferedKick;
    private float bufferedAttackTimer;
    private bool ignorePhysicalFighterCollisions = true;

    private float horizontalVelocityBeforeHitstop;
    private bool hasStoredHitstopVelocity;

    #endregion

    #region UI Animation And Sound Settings

    [Header("Animation")]
    [Tooltip("Should this fighter be animated")]
    [SerializeField] private bool animateFighter = true;

    [Tooltip("Animator component used for fighter model")]
    [SerializeField] private Animator fighterAnim;

    [Tooltip("Should fighter shuffle or walk forward")]
    [SerializeField] private bool walkNormal = false;

    [Tooltip("The physical transform of the fighter's 3D model")]
    [SerializeField] private Transform fighterModel;

    [Tooltip("Full Animator state path used when a new round begins")]
    [SerializeField]
    private string standingIdleStateName =
        "FighterMovement.Standing.Fighter_Idle_Standing 0";

    [Tooltip("Animator trigger used to play the round-win celebration")]
    [SerializeField] private string celebrationTriggerName = "celebrate";

    [Header("Attack Animation Speeds")]
    [Tooltip("Animation speed for standing punch")]
    [SerializeField] private float standingPunchAnimationSpeed = 1.25f;

    [Tooltip("Animation speed for crouching punch")]
    [SerializeField] private float crouchingPunchAnimationSpeed = 1.35f;

    [Tooltip("Animation speed for jumping punch")]
    [SerializeField] private float jumpingPunchAnimationSpeed = 1.15f;

    [Tooltip("Animation speed for standing kick")]
    [SerializeField] private float standingKickAnimationSpeed = 0.9f;

    [Tooltip("Animation speed for crouching kick")]
    [SerializeField] private float crouchingKickAnimationSpeed = 1f;

    [Tooltip("Animation speed for jumping kick")]
    [SerializeField] private float jumpingKickAnimationSpeed = 0.85f;

    [Tooltip("Animation speed for grab")]
    [SerializeField] private float grabAnimationSpeed = 0.9f;

    [Tooltip("Animation speed for special")]
    [SerializeField] private float specialAnimationSpeed = 0.75f;

    [Header("Sound Effects")]
    [Tooltip("Sound played when this fighter performs a punch attack")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip attackMissSound;
    [SerializeField] private AudioClip punchHitSound;
    [SerializeField] private AudioClip kickHitSound;
    [SerializeField] private AudioClip blockSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip jumpLandSound;
    [SerializeField] private AudioClip grabSound;

    #endregion

    #region Runtime State

    private bool isShuffling; //for animation script to read player state -HG
    private bool isGrounded;
    private bool isCrouching;
    private bool isBlocking;
    private bool isKnockedDown;
    private bool isRecovering;

    private float groundedPositionX;
    private bool hasGroundedPositionLock;

    public bool movingForward = false; //for tracking in dream sequence
    private float posXCurrent = 0;
    private float posXLast = 0;

    private int facingDirection = 1;

    private float blockStunTimer;
    private float groundedTimer;
    private float recoveryTimer;

    private float quickstepTimer;
    private float quickstepCooldownTimer;
    private float quickstepDirection;

    private float lastTapTime;
    private float lastTapDirection;
    private bool wasMovePressed;

    private float aiMoveInput;
    private bool aiJumpPressed;
    private bool aiPunchPressed;
    private bool aiKickPressed;
    private bool aiCrouchHeld;
    private bool aiGrabPressed;
    private bool aiSpecialPressed;

    private string pendingAttackName;
    private FighterMoveType pendingMoveType;
    private int pendingAttackDamage;
    private float pendingAttackRange;
    private AudioClip pendingHitSound;
    private bool hasPendingAttack;

    private bool hasPendingGrab;

    private bool isAttackAnimationPlaying; //prevents attack spam while an attack animation is still playing 
    private bool attackStartedAirborne;

    private Renderer[] fighterRenderers;
    private bool[] fighterRendererEnabledStates;
    private bool fighterPresentationHidden;

    private bool roundActive = true;
    private bool tutorialDamageImmune;
    private bool tutorialUnlimitedSpecials;



    #endregion

    #region Public API

    public bool IsRoundActive
    {
        get { return roundActive; }
    }

    public bool IsDreamTraversalMode
    {
        get { return dreamTraversalMode; }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    public bool IsCrouching
    {
        get { return isCrouching; }
    }

    public bool IsBlocking
    {
        get { return isBlocking; }
    }

    public bool IsQuickstepping
    {
        get { return quickstepTimer > 0f; }
    }


    public void SetTutorialDamageImmunity(bool immune)
    {
        tutorialDamageImmune = immune;
    }


    public void SetTutorialUnlimitedSpecials(bool unlimited)
    {
        tutorialUnlimitedSpecials = unlimited;
    }

    public event System.Action<FightCharacter, FighterMoveType, FighterMoveResult> MovePerformed;


    public void SetDreamTraversalMode(bool enabled)
    {
        dreamTraversalMode = enabled;


        walkNormal = enabled;

        if (!enabled)
            return;

        roundActive = true;
        ResetRoundState();

        blockStunTimer = 0f;
        isCrouching = false;
        isBlocking = false;

        quickstepTimer = 0f;
        quickstepCooldownTimer = 0f;
        quickstepDirection = 0f;

        hasPendingAttack = false;
        hasPendingGrab = false;
        isAttackAnimationPlaying = false;
        attackStartedAirborne = false;

        ClearBufferedAttack();

        facingDirection = 1;
        FlipModel(1f);
    }

    public void SetAIInput(float moveInput, bool jumpPressed, bool punchPressed, bool kickPressed, bool grabPressed, bool crouchHeld, bool specialPressed = false)
    {
        aiMoveInput = moveInput;
        aiJumpPressed = jumpPressed;
        aiPunchPressed = punchPressed;
        aiKickPressed = kickPressed;
        aiGrabPressed = grabPressed;
        aiCrouchHeld = crouchHeld;
        aiSpecialPressed = specialPressed;
    }

    public void SetRoundActive(bool isActive)
    {
        bool wasActive = roundActive;
        roundActive = isActive;

        if (!roundActive)
        {
            isShuffling = false;
            isCrouching = false;
            isBlocking = false;

            quickstepTimer = 0f;
            quickstepDirection = 0f;

            aiMoveInput = 0f;
            aiJumpPressed = false;
            aiPunchPressed = false;
            aiKickPressed = false;
            aiGrabPressed = false;
            aiSpecialPressed = false;
            aiCrouchHeld = false;

            if (rb != null)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0f;
                rb.linearVelocity = velocity;
            }

            if (fighterAnim != null)
            {
                fighterAnim.SetBool("shuffling", false);
                fighterAnim.SetBool("crouching", false);
                fighterAnim.SetBool("blocking", false);
            }

            return;
        }

        if (!wasActive)
        {
            ResetRoundState();
        }
    }

    /// <summary>
    /// Stops normal gameplay and displays this fighter's end round result
    /// The winner celebrates while the loser remains grounded until the next round
    /// </summary>
    public void SetRoundResult(bool wonRound)
    {
        SetRoundActive(false);

        isAttackAnimationPlaying = false;
        attackStartedAirborne = false;
        hasPendingAttack = false;
        hasPendingGrab = false;

        isInHitstop = false;
        hitstopTimer = 0f;
        animatorSpeedBeforeHitstop = 1f;
        hasStoredHitstopVelocity = false;
        horizontalVelocityBeforeHitstop = 0f;
        ClearBufferedAttack();

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
        }

        if (fighterAnim != null)
        {
            fighterAnim.speed = 1f;
            fighterAnim.ResetTrigger("punch");
            fighterAnim.ResetTrigger("kick");
            fighterAnim.ResetTrigger("grab");
            fighterAnim.ResetTrigger("special");
            fighterAnim.ResetTrigger("quickStep");
            fighterAnim.ResetTrigger("hurt");

            if (!string.IsNullOrWhiteSpace(celebrationTriggerName))
                fighterAnim.ResetTrigger(celebrationTriggerName);

            fighterAnim.SetBool("shuffling", false);
            fighterAnim.SetBool("crouching", false);
            fighterAnim.SetBool("blocking", false);
            fighterAnim.SetBool("jumping", false);
            fighterAnim.SetBool("recovering", false);
        }

        if (wonRound)
        {
            isKnockedDown = false;
            isRecovering = false;
            ReleaseGroundedHorizontalPosition();

            if (fighterAnim != null)
            {
                fighterAnim.SetBool("stunned", false);

                if (!string.IsNullOrWhiteSpace(celebrationTriggerName))
                    fighterAnim.SetTrigger(celebrationTriggerName);
            }

            return;
        }

        ApplyGroundedState();

        groundedTimer = float.PositiveInfinity;

        if (fighterAnim != null)
        {
            fighterAnim.SetBool("stunned", true);
            fighterAnim.SetBool("recovering", false);
            fighterAnim.Update(0f);
        }
    }

    public void SetPresentationVisible(bool isVisible)
    {
        if (!isVisible)
        {
            if (fighterPresentationHidden)
                return;

            fighterRenderers = GetComponentsInChildren<Renderer>(true);
            fighterRendererEnabledStates = new bool[fighterRenderers.Length];

            for (int i = 0; i < fighterRenderers.Length; i++)
            {
                Renderer fighterRenderer = fighterRenderers[i];

                if (fighterRenderer == null)
                    continue;

                fighterRendererEnabledStates[i] = fighterRenderer.enabled;
                fighterRenderer.enabled = false;
            }

            fighterPresentationHidden = true;
            return;
        }

        if (!fighterPresentationHidden)
            return;

        for (int i = 0; i < fighterRenderers.Length; i++)
        {
            Renderer fighterRenderer = fighterRenderers[i];

            if (fighterRenderer != null)
                fighterRenderer.enabled = fighterRendererEnabledStates[i];
        }

        fighterPresentationHidden = false;
    }

    public void ResetRoundState()
    {
        ReleaseGroundedHorizontalPosition();

        // Movement states
        isShuffling = false;
        isCrouching = false;
        isBlocking = false;

        // Attack states
        isAttackAnimationPlaying = false;
        attackStartedAirborne = false;
        hasPendingAttack = false;
        hasPendingGrab = false;

        // Quickstep states
        quickstepTimer = 0f;
        quickstepCooldownTimer = 0f;
        quickstepDirection = 0f;
        wasMovePressed = false;
        lastTapTime = 0f;
        lastTapDirection = 0f;

        // AI input
        aiMoveInput = 0f;
        aiJumpPressed = false;
        aiPunchPressed = false;
        aiKickPressed = false;
        aiGrabPressed = false;
        aiSpecialPressed = false;
        aiCrouchHeld = false;

        // Knockdown and recovery states
        isKnockedDown = false;
        isRecovering = false;
        groundedTimer = 0f;
        recoveryTimer = 0f;

        // Stop physical movement
        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
        }

        // Reset hitstop & buffered input 
        isInHitstop = false;
        hitstopTimer = 0f;
        animatorSpeedBeforeHitstop = 1f;
        ClearBufferedAttack();

        hasStoredHitstopVelocity = false;
        horizontalVelocityBeforeHitstop = 0f;

        // Reset Animator parameters
        ResetAnimatorForRound();
    }

    private void ResetAnimatorForRound()
    {
        if (fighterAnim == null)
            return;

        fighterAnim.speed = 1f;


        fighterAnim.ResetTrigger("punch");
        fighterAnim.ResetTrigger("kick");
        fighterAnim.ResetTrigger("grab");
        fighterAnim.ResetTrigger("special");
        fighterAnim.ResetTrigger("quickStep");
        fighterAnim.ResetTrigger("hurt");

        if (!string.IsNullOrWhiteSpace(celebrationTriggerName))
            fighterAnim.ResetTrigger(celebrationTriggerName);

        fighterAnim.SetBool("shuffling", false);
        fighterAnim.SetBool("crouching", false);
        fighterAnim.SetBool("blocking", false);
        fighterAnim.SetBool("jumping", false);
        fighterAnim.SetBool("recovering", false);
        fighterAnim.SetFloat("recoverySpeed", 1f);

        string resolvedIdleStateName = standingIdleStateName;
        int idleStateHash = string.IsNullOrWhiteSpace(resolvedIdleStateName)
            ? 0
            : Animator.StringToHash(resolvedIdleStateName);

        if (idleStateHash == 0 || !fighterAnim.HasState(0, idleStateHash))
        {
            string layerName = fighterAnim.GetLayerName(0);
            resolvedIdleStateName = layerName + "." + standingIdleStateName;
            idleStateHash = Animator.StringToHash(resolvedIdleStateName);
        }

        if (!fighterAnim.HasState(0, idleStateHash))
        {
            Debug.LogWarning(
                name + " could not find round start Animator state '" +
                standingIdleStateName + "'. Set Standing Idle State Name to its exact full Animator path.",
                this
            );
            return;
        }

        fighterAnim.Play(idleStateHash, 0, 0f);
        fighterAnim.SetBool("stunned", false);

        fighterAnim.Update(0f);
    }

    public void StartAttackAnimation()
    {
        if (dreamTraversalMode)
            return;

        isAttackAnimationPlaying = true; //locks attack input during attack animation 
        attackStartedAirborne = !isGrounded;

        if (rb != null && isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
        }
    }

    public void EndAttackAnimation()
    {
        isAttackAnimationPlaying = false; //unlocks attack input after animation finishes 
        attackStartedAirborne = false;

        if (fighterAnim != null)
        {
            if (isInHitstop)
                animatorSpeedBeforeHitstop = 1f;
            else
                fighterAnim.speed = 1f;
        }

        hasPendingAttack = false;
        hasPendingGrab = false;
    }

    private void SetAttackAnimationSpeed(float speed)
    {
        if (fighterAnim == null)
            return;

        fighterAnim.speed = speed;
    }

    private float GetPunchAnimationSpeed()
    {
        if (!isGrounded)
            return jumpingPunchAnimationSpeed;

        if (isCrouching)
            return crouchingPunchAnimationSpeed;

        return standingPunchAnimationSpeed;
    }

    private float GetKickAnimationSpeed()
    {
        if (!isGrounded)
            return jumpingKickAnimationSpeed;

        if (isCrouching)
            return crouchingKickAnimationSpeed;

        return standingKickAnimationSpeed;
    }

    public void PerformAttackHit()
    {
        if (dreamTraversalMode)
            return;

        if (!hasPendingAttack)
            return;

        hasPendingAttack = false;

        FighterMoveResult result = TryHitOpponent(
            pendingAttackName,
            pendingAttackDamage,
            pendingAttackRange
        );

        if (result == FighterMoveResult.Hit)
        {
            StartLocalHitstop(attackerHitstopTime);
            PlayConnectedAttackShake();
        }
        else if (result == FighterMoveResult.Blocked)
            StartLocalHitstop(blockedHitstopTime);

        PlayAttackResultSound(result, pendingHitSound);

        MovePerformed?.Invoke(this, pendingMoveType, result);
    }

    public void PerformGrabHit()
    {
        if (dreamTraversalMode)
            return;

        if (!hasPendingGrab)
            return;

        hasPendingGrab = false;

        FighterMoveResult result = TryGrabOpponent();

        if (result == FighterMoveResult.Hit)
        {
            PlaySound(grabSound);
            PlayConnectedAttackShake();
        }
        else
            PlaySound(attackMissSound);

        MovePerformed?.Invoke(this, FighterMoveType.Grab, result);
    }

    public FighterMoveResult ReceiveGrab(FightCharacter attacker, int damage)
    {
        if (dreamTraversalMode)
            return FighterMoveResult.Miss;

        if (isKnockedDown || isRecovering)
            return FighterMoveResult.Miss;

        if (quickstepTimer > 0f)
            return FighterMoveResult.Miss;

        if (attacker == null)
            return FighterMoveResult.Miss;

        if (health == null)
        {
            Debug.Log(name + " has no FighterHealth script.");
            return FighterMoveResult.Miss;
        }

        if (!tutorialDamageImmune)
            health.TakeDamage(damage, true);

        SwitchSidesWithAttacker(attacker);
        ApplyGroundedState();

        return FighterMoveResult.Hit;
    }

    /// <summary>
    /// If blocking correctly, applies chip damage, block stun, and small knockback.
    /// If not blocking, applies full damage.
    /// </summary>
    public FighterMoveResult ReceiveAttack(
        int damage,
        Vector3 attackerPosition,
        FightCharacter attacker)
    {
        if (dreamTraversalMode)
            return FighterMoveResult.Miss;

        if (isKnockedDown)
            return FighterMoveResult.Miss;

        if (quickstepTimer > 0f)
            return FighterMoveResult.Miss;

        if (IsBlockingAttack(attackerPosition))
        {
            ApplyBlockedAttack(attackerPosition, attacker);

            StartLocalHitstop(blockedHitstopTime);

            return FighterMoveResult.Blocked;
        }

        if (health == null)
        {
            Debug.Log(name + " has no FighterHealth script");
            return FighterMoveResult.Miss;
        }

        if (!tutorialDamageImmune)
            health.TakeDamage(damage, true);
        ApplyDamage();
        ApplyHitPushback(attackerPosition);

        StartLocalHitstop(defenderHitstopTime);

        return FighterMoveResult.Hit;
    }

    private void StartLocalHitstop(float duration)
    {
        if (duration <= 0f)
            return;

        if (!isInHitstop)
        {
            isInHitstop = true;

            if (fighterAnim != null)
            {
                animatorSpeedBeforeHitstop = fighterAnim.speed;
                fighterAnim.speed = 0f;
            }

            if (rb != null)
            {
                horizontalVelocityBeforeHitstop = rb.linearVelocity.x;
                hasStoredHitstopVelocity = true;

                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0f;
                rb.linearVelocity = velocity;
            }
        }

        hitstopTimer = Mathf.Max(hitstopTimer, duration);
    }

    private void UpdateLocalHitstop()
    {
        if (!isInHitstop)
            return;

        hitstopTimer -= Time.unscaledDeltaTime;

        if (hitstopTimer > 0f)
            return;

        hitstopTimer = 0f;
        isInHitstop = false;

        if (fighterAnim != null)
            fighterAnim.speed = animatorSpeedBeforeHitstop;

        if (fighterAnim != null)
            fighterAnim.speed = animatorSpeedBeforeHitstop;

        if (rb != null && hasStoredHitstopVelocity)
        {
            Vector3 velocity = rb.linearVelocity;

            velocity.x = horizontalVelocityBeforeHitstop * 0.65f;

            rb.linearVelocity = velocity;
        }

        hasStoredHitstopVelocity = false;
        horizontalVelocityBeforeHitstop = 0f;
    }

    private void CaptureHitstopInput()
    {
        if (controlledByAI || input == null)
            return;

        if (input.PunchPressed)
        {
            bufferedPunch = true;
            bufferedKick = false;
            bufferedAttackTimer = hitstopInputBufferTime;
        }
        else if (input.KickPressed)
        {
            bufferedKick = true;
            bufferedPunch = false;
            bufferedAttackTimer = hitstopInputBufferTime;
        }
    }

    private void UpdateBufferedAttack()
    {
        if (!bufferedPunch && !bufferedKick)
            return;

        bufferedAttackTimer -= Time.unscaledDeltaTime;

        if (bufferedAttackTimer <= 0f)
            ClearBufferedAttack();
    }

    private bool TryPerformBufferedAttack()
    {
        if ((!bufferedPunch && !bufferedKick) || !CanStartAttack())
            return false;

        bool performPunch = bufferedPunch;

        ClearBufferedAttack();

        if (performPunch)
            Punch();
        else
            Kick();

        return true;
    }

    private void ClearBufferedAttack()
    {
        bufferedPunch = false;
        bufferedKick = false;
        bufferedAttackTimer = 0f;
    }

    public void FlipModel(float direction)
    {
        //flips the fighter model on grab - HG
        if (fighterModel != null)
        {
            Vector3 scale = fighterModel.localScale;
            scale.x = Mathf.Abs(scale.x); //re-orient
            scale.x *= direction;

            if (scale.x != fighterModel.localScale.x)
            {
                fighterModel.localScale = scale;
            }
        }
    }

    #endregion

    #region Unity Lifecycle

    private void Reset()
    {
        AssignMissingReferences();
    }

    private void Awake()
    {
        AssignMissingReferences();
    }

    private void Start()
    {
        ConfigurePhysicalFighterCollisions();

        if (dreamTraversalMode)
            SetDreamTraversalMode(true);
    }

    private void Update()
    {

        UpdateIsMovingForward(); //check if fighter is moving forward on X axis - HG
        UpdateGrounded();
        UpdateFacingDirection();


        UpdateBufferedAttack();

        if (isInHitstop)
            CaptureHitstopInput();

        UpdateLocalHitstop();

        if (!roundActive)
            return;

        if (isInHitstop)
            return;

        UpdateBlockStunTimer();
        UpdateQuickstepTimers();
        UpdateGroundedStateTimers();

        if (animateFighter && fighterAnim != null)
            UpdateAnimation();

        if (blockStunTimer > 0f)
            return;

        if (isKnockedDown)
        {
            CheckForManualRecovery();
            return;
        }

        if (isRecovering)
            return;

        if (TryPerformBufferedAttack())
            return;

        ReadActions();
    }


    private void FixedUpdate()
    {
        if (!roundActive)
            return;

        MaintainGroundedHorizontalPosition();

        if (isInHitstop)
        {
            if (rb != null)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0f;
                rb.linearVelocity = velocity;
            }

            ResolveFighterPushboxes();
            return;
        }

        ApplyJumpGravity();
        Move();
        LimitCurrentVelocityIntoOpponentPushbox();
        ResolveFighterPushboxes();
    }

    #endregion

    #region Setup

    private void AssignMissingReferences()
    {
        rb ??= GetComponent<Rigidbody>();
        input ??= GetComponent<FighterInput>();
        health ??= GetComponent<FighterHealth>();
        audioSource ??= GetComponent<AudioSource>();
        superMeter ??= GetComponent<FighterSuperMeter>();
        fightCamera ??= FindAnyObjectByType<FightCameraFollow>();
    }

    private void ConfigurePhysicalFighterCollisions()
    {
        if (!ignorePhysicalFighterCollisions || opponent == null)
            return;

        Collider[] thisColliders = GetComponentsInChildren<Collider>(true);
        Collider[] opponentColliders = opponent.GetComponentsInChildren<Collider>(true);

        foreach (Collider thisCollider in thisColliders)
        {
            foreach (Collider opponentCollider in opponentColliders)
            {
                if (thisCollider != null && opponentCollider != null &&
                    thisCollider != opponentCollider)
                {
                    Physics.IgnoreCollision(thisCollider, opponentCollider, true);
                }
            }
        }
    }

    #endregion

    #region Input Reading

    private void ReadActions()
    {
        if (!TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool grabPressed, out bool specialPressed, out bool crouchHeld))
            return;

        if (dreamTraversalMode)
        {
            isCrouching = false;
            isBlocking = false;
            quickstepTimer = 0f;
            quickstepDirection = 0f;

            ClearAIButtonInputs();
            return;
        }

        CheckQuickstepInput(moveInput);

        bool holdingBack = IsHoldingBack(moveInput);

        isCrouching = quickstepTimer <= 0f && crouchHeld && isGrounded;
        isBlocking = quickstepTimer <= 0f && holdingBack && isGrounded;

        if (quickstepTimer > 0f)
        {
            isBlocking = false;
            isCrouching = false;

            ClearAIButtonInputs();
            return;
        }

        if (jumpPressed && isGrounded && !isCrouching && !isBlocking)
        {
            Jump(moveInput);
            ClearAIButtonInputs();
            return;
        }

        if (specialPressed)
        {
            Special();
            ClearAIButtonInputs();
            return;
        }

        if (grabPressed)
        {
            Grab();
            ClearAIButtonInputs();
            return;
        }

        if (punchPressed)
        {
            Punch();
            ClearAIButtonInputs();
            return;
        }

        if (kickPressed)
        {
            Kick();
            ClearAIButtonInputs();
            return;
        }

        ClearAIButtonInputs();
    }

    private bool TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool grabPressed, out bool specialPressed, out bool crouchHeld)
    {
        if (controlledByAI)
        {
            moveInput = aiMoveInput;
            jumpPressed = aiJumpPressed;
            punchPressed = aiPunchPressed;
            kickPressed = aiKickPressed;
            grabPressed = aiGrabPressed;
            specialPressed = aiSpecialPressed;
            crouchHeld = aiCrouchHeld;
            return true;
        }

        if (input == null)
        {
            moveInput = 0f;
            jumpPressed = false;
            punchPressed = false;
            kickPressed = false;
            grabPressed = false;
            specialPressed = false;
            crouchHeld = false;
            return false;
        }

        moveInput = input.Move;
        jumpPressed = input.JumpPressed;
        punchPressed = input.PunchPressed;
        kickPressed = input.KickPressed;
        grabPressed = input.GrabPressed;
        specialPressed = input.SpecialPressed;
        crouchHeld = input.CrouchHeld;

        return true;
    }

    /// <summary>
    /// Movement input is not cleared because the AI may need to keep holding a direction
    /// </summary>
    private void ClearAIButtonInputs()
    {
        if (!controlledByAI)
            return;

        aiJumpPressed = false;
        aiPunchPressed = false;
        aiKickPressed = false;
        aiGrabPressed = false;
        aiSpecialPressed = false;
    }

    #endregion

    #region Movement

    private void Move()
    {
        isShuffling = false;

        if (rb == null || blockStunTimer > 0f || isKnockedDown || isRecovering || isAttackAnimationPlaying)
            return;

        if (quickstepTimer > 0f)
        {
            Vector3 quickstepVelocity = rb.linearVelocity;
            quickstepVelocity.x = quickstepDirection * quickstepSpeed;
            rb.linearVelocity = quickstepVelocity;
            return;
        }

        if (!isGrounded)
            return;

        if (!TryGetCurrentInput(out float moveInput, out _, out _, out _, out _, out _, out _))
            return;

        float horizontal = Mathf.Abs(moveInput) < inputDeadZone
            ? 0f
            : moveInput;

        if (dreamTraversalMode)
            horizontal = Mathf.Max(0f, horizontal);

        float currentMoveSpeed = moveSpeed;

        if (!isBlocking) //only shuffling is character is moving forward -HG
        {
            isShuffling = Mathf.Abs(horizontal) >= inputDeadZone; //check if player is moving or still - HG
        }

        if (isBlocking)
            currentMoveSpeed = blockMoveSpeed;
        else if (isCrouching)
            currentMoveSpeed = crouchMoveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = LimitVelocityIntoOpponentPushbox(horizontal * currentMoveSpeed);
        rb.linearVelocity = velocity;
    }

    private struct FighterPushbox
    {
        public Vector2 center;
        public Vector2 size;

        public FighterPushbox(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = size;
        }
    }

    /// <summary>
    /// Caps walking speed before the next physics step so fighter roots cannot cross while their pushboxes
    /// </summary>
    private float LimitVelocityIntoOpponentPushbox(float horizontalVelocity)
    {
        if (dreamTraversalMode)
            return horizontalVelocity;

        if (opponent == null || Mathf.Approximately(horizontalVelocity, 0f))
            return horizontalVelocity;

        if (!opponent.TryGetComponent(out FightCharacter opponentCharacter))
            return horizontalVelocity;

        FighterPushbox thisPushbox = GetCurrentPushbox();
        FighterPushbox opponentPushbox = opponentCharacter.GetCurrentPushbox();

        if (!PushboxesOverlapVertically(thisPushbox, opponentPushbox))
            return horizontalVelocity;

        float differenceX = opponentPushbox.center.x - thisPushbox.center.x;

        if (Mathf.Approximately(differenceX, 0f))
            return 0f;

        if (Mathf.Sign(horizontalVelocity) != Mathf.Sign(differenceX))
            return horizontalVelocity;

        float requiredDistance = GetRequiredHorizontalDistance(
            thisPushbox,
            opponentPushbox,
            opponentCharacter
        );

        float remainingDistance = Mathf.Max(
            0f,
            Mathf.Abs(differenceX) - requiredDistance
        );

        bool opponentIsClosingGap =
            opponentCharacter.IsTryingToMoveTowardPosition(thisPushbox.center.x);

        float movementShare = opponentIsClosingGap ? 0.5f : 1f;
        float maximumVelocityThisStep =
            remainingDistance / Time.fixedDeltaTime * movementShare;

        return Mathf.Sign(horizontalVelocity) * Mathf.Min(
            Mathf.Abs(horizontalVelocity),
            maximumVelocityThisStep
        );
    }

    private void LimitCurrentVelocityIntoOpponentPushbox()
    {
        if (rb == null)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = LimitVelocityIntoOpponentPushbox(velocity.x);
        rb.linearVelocity = velocity;
    }

    private bool IsTryingToMoveTowardPosition(float targetX)
    {
        Vector3 rootPosition = rb != null ? rb.position : transform.position;
        float differenceX = targetX - rootPosition.x;

        if (Mathf.Approximately(differenceX, 0f))
            return false;

        float directionToTarget = Mathf.Sign(differenceX);

        if (quickstepTimer > 0f &&
            Mathf.Sign(quickstepDirection) == directionToTarget)
        {
            return true;
        }

        if (!isKnockedDown && !isRecovering &&
            blockStunTimer <= 0f && !isAttackAnimationPlaying &&
            TryGetCurrentInput(
                out float moveInput,
                out _, out _, out _, out _, out _, out _))
        {
            if (Mathf.Abs(moveInput) >= inputDeadZone &&
                Mathf.Sign(moveInput) == directionToTarget)
            {
                return true;
            }
        }

        return rb != null &&
               rb.linearVelocity.x * directionToTarget > 0.01f;
    }

    private FighterPushbox GetCurrentPushbox()
    {
        Vector2 size = GetCurrentPushboxSize();
        Vector3 rootPosition = rb != null ? rb.position : transform.position;

        return new FighterPushbox(
            new Vector2(rootPosition.x, rootPosition.y + size.y * 0.5f),
            size
        );
    }

    private Vector2 GetCurrentPushboxSize()
    {
        Vector2 size;

        if (isKnockedDown)
            size = groundedPushboxSize;
        else if (isRecovering)
            size = recoveringPushboxSize;
        else if (IsUsingJumpingPushbox())
            size = jumpingPushboxSize;
        else
            size = standingPushboxSize;

        size.x = Mathf.Max(0.05f, size.x);
        size.y = Mathf.Max(0.05f, size.y);
        return size;
    }

    private bool IsUsingJumpingPushbox()
    {
        return !isGrounded ||
               (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.05f);
    }

    private static bool PushboxesOverlapVertically(
        FighterPushbox first,
        FighterPushbox second)
    {
        float firstBottom = first.center.y - first.size.y * 0.5f;
        float firstTop = first.center.y + first.size.y * 0.5f;
        float secondBottom = second.center.y - second.size.y * 0.5f;
        float secondTop = second.center.y + second.size.y * 0.5f;

        return Mathf.Min(firstTop, secondTop) >
               Mathf.Max(firstBottom, secondBottom);
    }

    private float GetRequiredHorizontalDistance(
        FighterPushbox first,
        FighterPushbox second,
        FightCharacter opponentCharacter)
    {
        float separationBuffer = Mathf.Max(
            0f,
            Mathf.Max(
                pushboxSeparationBuffer,
                opponentCharacter != null
                    ? opponentCharacter.pushboxSeparationBuffer
                    : 0f
            )
        );

        return (first.size.x + second.size.x) * 0.5f + separationBuffer;
    }

    /// <summary>
    /// Pushes the two fighters apart if their pushboxes overlap horizontally while overlapping verticall 
    /// </summary>
    private void ResolveFighterPushboxes()
    {
        if (dreamTraversalMode)
            return;

        if (rb == null || opponent == null)
            return;

        if (!opponent.TryGetComponent(out FightCharacter opponentCharacter) ||
            opponentCharacter.rb == null || !opponentCharacter.roundActive)
        {
            return;
        }

        FighterPushbox thisPushbox = GetCurrentPushbox();
        FighterPushbox opponentPushbox = opponentCharacter.GetCurrentPushbox();

        if (!PushboxesOverlapVertically(thisPushbox, opponentPushbox))
            return;

        float differenceX = opponentPushbox.center.x - thisPushbox.center.x;
        float requiredDistance = GetRequiredHorizontalDistance(
            thisPushbox,
            opponentPushbox,
            opponentCharacter
        );
        float overlap = requiredDistance - Mathf.Abs(differenceX);

        if (overlap <= 0f)
            return;

        float directionToOpponent = Mathf.Approximately(differenceX, 0f)
            ? facingDirection
            : Mathf.Sign(differenceX);

        bool thisLocked = IsPushboxHorizontallyLocked();
        bool opponentLocked = opponentCharacter.IsPushboxHorizontallyLocked();

        if (thisLocked && opponentLocked)
            return;

        float thisShare = thisLocked ? 0f : opponentLocked ? 1f : 0.5f;
        float opponentShare = opponentLocked ? 0f : thisLocked ? 1f : 0.5f;

        MovePushboxRoot(-directionToOpponent * overlap * thisShare);
        opponentCharacter.MovePushboxRoot(
            directionToOpponent * overlap * opponentShare
        );

        StopHorizontalVelocityToward(directionToOpponent);
        opponentCharacter.StopHorizontalVelocityToward(-directionToOpponent);
    }

    private bool IsPushboxHorizontallyLocked()
    {
        return isKnockedDown || isRecovering;
    }

    private void MovePushboxRoot(float amount)
    {
        if (rb == null || Mathf.Approximately(amount, 0f))
            return;

        Vector3 position = rb.position;
        position.x += amount;
        rb.position = position;
    }

    private void StopHorizontalVelocityToward(float direction)
    {
        if (rb == null)
            return;

        Vector3 velocity = rb.linearVelocity;

        if (velocity.x * direction <= 0f)
            return;

        velocity.x = 0f;
        rb.linearVelocity = velocity;
    }

    private void Jump(float moveInput)
    {
        PlaySound(jumpSound);

        if (rb == null)
            return;


        isGrounded = false;
        isCrouching = false;
        isBlocking = false;

        float jumpHorizontalDirection = Mathf.Abs(moveInput) < inputDeadZone ? 0f : Mathf.Sign(moveInput);

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        velocity.x = jumpHorizontalDirection * moveSpeed;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animateFighter && fighterAnim != null)
        {
            fighterAnim.SetBool("crouching", false);
            fighterAnim.SetBool("blocking", false);
            fighterAnim.SetBool("jumping", true);
        }
    }

    private void ApplyJumpGravity()
    {
        if (rb == null)
            return;

        float verticalSpeed = rb.linearVelocity.y;
        float gravityMultiplier;

        if (verticalSpeed > 0.01f)
        {
            gravityMultiplier = risingGravityMultiplier;
        }
        else if (!isGrounded && verticalSpeed < -0.01f)
        {
            gravityMultiplier = fallingGravityMultiplier;
        }
        else
        {
            return;
        }

        rb.AddForce(
            Physics.gravity * (gravityMultiplier - 1f),
            ForceMode.Acceleration
        );
    }

    private void CheckQuickstepInput(float moveInput)
    {
        if (!isGrounded || isCrouching || blockStunTimer > 0f || isKnockedDown || isRecovering)
            return;

        if (quickstepCooldownTimer > 0f || quickstepTimer > 0f)
            return;

        bool movePressed = Mathf.Abs(moveInput) >= inputDeadZone;

        if (movePressed && !wasMovePressed)
        {
            float tapDirection = Mathf.Sign(moveInput);

            if (tapDirection == lastTapDirection &&
                Time.time - lastTapTime <= doubleTapWindow)
            {
                StartQuickstep(tapDirection);
                lastTapTime = 0f;
                lastTapDirection = 0f;
            }
            else
            {
                lastTapTime = Time.time;
                lastTapDirection = tapDirection;
            }
        }

        wasMovePressed = movePressed;
    }

    private void StartQuickstep(float direction)
    {
        quickstepDirection = direction;
        quickstepTimer = quickstepTime;
        quickstepCooldownTimer = quickstepCooldown;

        isBlocking = false;
        isCrouching = false;

        if (animateFighter) { fighterAnim.SetTrigger("quickStep"); }
    }

    private void UpdateQuickstepTimers()
    {
        if (quickstepTimer > 0f)
        {
            quickstepTimer -= Time.deltaTime;

            if (quickstepTimer <= 0f && rb != null)
            {
                Vector3 stopVelocity = rb.linearVelocity;
                stopVelocity.x = 0f;
                rb.linearVelocity = stopVelocity;
            }
        }

        if (quickstepCooldownTimer > 0f)
            quickstepCooldownTimer -= Time.deltaTime;
    }

    //updating whether the player is moving forward or not for dream seqeuence stuff - HG
    private void UpdateIsMovingForward()
    {
        posXCurrent = transform.localPosition.x;

        movingForward = (posXCurrent > posXLast + 0.05f) ? true : false;

        posXLast = posXCurrent;
    }

    #endregion

    #region Combat Actions

    private void Punch()
    {
        if (!CanStartAttack())
            return;

        StartAttackAnimation(); //locks immediately so button spam cannot restart the animation

        string punchType = GetPunchType();
        FighterMoveType moveType = GetPunchMoveType();

        int damage = GetPunchDamage();
        float range = GetPunchRange();

        StorePendingAttack(
            punchType,
            moveType,
            damage,
            range,
            punchHitSound
        );

        if (animateFighter && fighterAnim != null)
        {
            SetAttackAnimationSpeed(GetPunchAnimationSpeed());
            fighterAnim.SetTrigger("punch");
        }
    }

    private void Kick()
    {
        if (!CanStartAttack())
            return;

        StartAttackAnimation(); //locks immediately so button spam cannot restart the animation

        string kickType = GetKickType();
        FighterMoveType moveType = GetKickMoveType();

        int damage = GetKickDamage();
        float range = GetKickRange();

        StorePendingAttack(
            kickType,
            moveType,
            damage,
            range,
            kickHitSound
        );

        if (animateFighter && fighterAnim != null)
        {
            SetAttackAnimationSpeed(GetKickAnimationSpeed());
            fighterAnim.SetTrigger("kick");
        }
    }

    private void Grab()
    {
        if (!CanStartAttack())
            return;

        if (!isGrounded || isCrouching || isBlocking || isKnockedDown || isRecovering)
        {
            PlaySound(attackMissSound);
            MovePerformed?.Invoke(this, FighterMoveType.Grab, FighterMoveResult.Miss);
            return;
        }

        StartAttackAnimation(); //locks immediately so button spam cannot restart the animation 

        hasPendingGrab = true;

        if (animateFighter && fighterAnim != null)
        {
            SetAttackAnimationSpeed(grabAnimationSpeed);
            fighterAnim.SetTrigger("grab");
        }
    }

    private void Special()
    {
        if (!CanStartAttack())
            return;

        if (superMeter == null)
        {
            Debug.Log(name + " has no FighterSuperMeter script.");
            return;
        }

        if (!tutorialUnlimitedSpecials && !superMeter.TrySpendSpecial())
        {
            PlaySound(attackMissSound);
            MovePerformed?.Invoke(this, FighterMoveType.Special, FighterMoveResult.Miss);
            return;
        }

        StartAttackAnimation();

        StorePendingAttack(
            "Special",
            FighterMoveType.Special,
            specialDamage,
            specialRange,
            specialHitSound != null ? specialHitSound : kickHitSound
        );

        if (animateFighter && fighterAnim != null)
        {
            SetAttackAnimationSpeed(specialAnimationSpeed);
            fighterAnim.SetTrigger("special");
        }
    }

    private bool CanStartAttack()
    {
        return !dreamTraversalMode
            && roundActive
            && !isAttackAnimationPlaying
            && !isKnockedDown
            && !isRecovering
            && blockStunTimer <= 0f;
    }

    private void StorePendingAttack(
        string attackName,
        FighterMoveType moveType,
        int damage,
        float range,
        AudioClip hitSound)
    {
        pendingAttackName = attackName;
        pendingMoveType = moveType;
        pendingAttackDamage = damage;
        pendingAttackRange = range;
        pendingHitSound = hitSound;
        hasPendingAttack = true;
    }

    #endregion

    #region Combat Resolution

    private FighterMoveResult TryHitOpponent(string attackName, int damage, float range)
    {
        if (opponent == null)
        {
            Debug.Log("No opponent assigned");
            return FighterMoveResult.Miss;
        }

        float distanceToOpponent = Vector3.Distance(transform.position, opponent.position);

        if (distanceToOpponent > range)
        {
            Debug.Log(attackName + " missed");
            return FighterMoveResult.Miss;
        }

        if (!opponent.TryGetComponent(out FightCharacter opponentCharacter))
        {
            Debug.Log("Opponent has no FightCharacter script.");
            return FighterMoveResult.Miss;
        }

        return opponentCharacter.ReceiveAttack(damage, transform.position, this);
    }

    private FighterMoveResult TryGrabOpponent()
    {
        if (opponent == null)
        {
            Debug.Log("No opponent assigned");
            return FighterMoveResult.Miss;
        }

        float distanceToOpponent = Vector3.Distance(transform.position, opponent.position);

        if (distanceToOpponent > grabRange)
        {
            Debug.Log("Grab missed");
            return FighterMoveResult.Miss;
        }

        if (!opponent.TryGetComponent(out FightCharacter opponentCharacter))
        {
            Debug.Log("Opponent has no FightCharacter script.");
            return FighterMoveResult.Miss;
        }

        return opponentCharacter.ReceiveGrab(this, grabDamage);
    }

    private void PlayConnectedAttackShake()
    {
        if (fightCamera == null)
            fightCamera = FindAnyObjectByType<FightCameraFollow>();

        if (fightCamera != null)
            fightCamera.Shake(hitShakeDuration, hitShakeStrength);
    }

    /// <summary>
    /// Grabs ignore block, deal damage, switch the fighters' positions, and put this fighter into grounded state.
    /// </summary>
    private void SwitchSidesWithAttacker(FightCharacter attacker)
    {
        if (attacker == null)
            return;

        Vector3 oldAttackerPosition = attacker.transform.position;
        Vector3 oldDefenderPosition = transform.position;

        float directionFromAttackerToDefender =
            Mathf.Sign(oldDefenderPosition.x - oldAttackerPosition.x);

        if (Mathf.Approximately(directionFromAttackerToDefender, 0f))
            directionFromAttackerToDefender = attacker.facingDirection;

        float postGrabPushboxDistance =
            (attacker.GetCurrentPushboxSize().x +
             Mathf.Max(0.05f, groundedPushboxSize.x)) * 0.5f +
            Mathf.Max(
                pushboxSeparationBuffer,
                attacker.pushboxSeparationBuffer
            );

        float separation = Mathf.Max(
            grabSideSwitchOffset,
            postGrabPushboxDistance
        );

        float midpointX =
            (oldAttackerPosition.x + oldDefenderPosition.x) * 0.5f;

        Vector3 newAttackerPosition = oldAttackerPosition;
        Vector3 newDefenderPosition = oldDefenderPosition;

        newAttackerPosition.x =
            midpointX + directionFromAttackerToDefender * separation * 0.5f;

        newDefenderPosition.x =
            midpointX - directionFromAttackerToDefender * separation * 0.5f;

        if (attacker.rb != null)
        {
            Vector3 attackerVelocity = attacker.rb.linearVelocity;
            attackerVelocity.x = 0f;
            attacker.rb.linearVelocity = attackerVelocity;

            attacker.rb.position = newAttackerPosition;
        }
        else
        {
            attacker.transform.position = newAttackerPosition;
        }

        if (rb != null)
        {
            Vector3 defenderVelocity = rb.linearVelocity;
            defenderVelocity.x = 0f;
            rb.linearVelocity = defenderVelocity;

            rb.position = newDefenderPosition;
        }
        else
        {
            transform.position = newDefenderPosition;
        }

        float defenderFacingDirection =
            Mathf.Sign(newAttackerPosition.x - newDefenderPosition.x);

        FlipModel(defenderFacingDirection);
        attacker.FlipModel(-defenderFacingDirection);
    }

    /// <summary>
    /// Chip damage cannot defeat the fighter.
    /// </summary>
    private void ApplyBlockedAttack(Vector3 attackerPosition, FightCharacter attacker)
    {
        if (health != null && !tutorialDamageImmune)
            health.TakeDamage(blockChipDamage, false);

        ApplyBlockStun();
        ApplyBlockKnockback(attackerPosition);

        if (attacker != null)
            attacker.ApplyBlockRecoil(transform.position);

        Debug.Log(tutorialDamageImmune
            ? name + " blocked the tutorial attack with no damage."
            : name + " blocked the attack and took chip damage.");
    }

    private void ApplyDamage()
    {
        if (animateFighter)
        {
            fighterAnim.SetTrigger("hurt");
        }
    }

    private void ApplyBlockStun()
    {
        blockStunTimer = blockStunTime;
    }

    private void ApplyBlockKnockback(Vector3 attackerPosition)
    {
        if (rb == null)
            return;

        float directionAwayFromAttacker = Mathf.Sign(transform.position.x - attackerPosition.x);

        Vector3 velocity = rb.linearVelocity;
        velocity.x = directionAwayFromAttacker * blockKnockbackForce;
        rb.linearVelocity = velocity;
    }

    private void ApplyBlockRecoil(Vector3 blockerPosition)
    {
        if (rb == null)
            return;

        float directionAwayFromBlocker = Mathf.Sign(transform.position.x - blockerPosition.x);

        Vector3 velocity = rb.linearVelocity;
        velocity.x = directionAwayFromBlocker * attackerBlockRecoilForce;
        rb.linearVelocity = velocity;
    }

    private void ApplyHitPushback(Vector3 attackerPosition)
    {
        if (rb == null)
            return;

        float directionAwayFromAttacker = Mathf.Sign(transform.position.x - attackerPosition.x);

        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.right * directionAwayFromAttacker * hitPushbackForce, ForceMode.Impulse);
    }

    #endregion

    #region Attack Type Helpers

    private FighterMoveType GetPunchMoveType()
    {
        if (!isGrounded)
            return FighterMoveType.JumpingPunch;

        if (isCrouching)
            return FighterMoveType.CrouchingPunch;

        return FighterMoveType.StandingPunch;
    }

    private FighterMoveType GetKickMoveType()
    {
        if (!isGrounded)
            return FighterMoveType.JumpingKick;

        if (isCrouching)
            return FighterMoveType.CrouchingKick;

        return FighterMoveType.StandingKick;
    }

    private string GetPunchType()
    {
        if (!isGrounded)
            return "Jumping Punch";

        if (isCrouching)
            return "Crouching Punch";

        return "Standing Punch";
    }

    private string GetKickType()
    {
        if (!isGrounded)
            return "Jumping Kick";

        if (isCrouching)
            return "Crouching Kick";

        return "Standing Kick";
    }

    private int GetPunchDamage()
    {
        if (!isGrounded)
            return jumpingPunchDamage;

        if (isCrouching)
            return crouchingPunchDamage;

        return standingPunchDamage;
    }

    private int GetKickDamage()
    {
        if (!isGrounded)
            return jumpingKickDamage;

        if (isCrouching)
            return crouchingKickDamage;

        return standingKickDamage;
    }

    private float GetPunchRange()
    {
        if (!isGrounded)
            return jumpingPunchRange;

        if (isCrouching)
            return crouchingPunchRange;

        return standingPunchRange;
    }

    private float GetKickRange()
    {
        if (!isGrounded)
            return jumpingKickRange;

        if (isCrouching)
            return crouchingKickRange;

        return standingKickRange;
    }

    #endregion

    #region Defensive State Helpers

    private bool IsBlockingAttack(Vector3 attackerPosition)
    {
        return isBlocking
            && IsAttackFromFront(attackerPosition);
    }

    private bool IsAttackFromFront(Vector3 attackerPosition)
    {
        float directionToAttacker = Mathf.Sign(attackerPosition.x - transform.position.x);
        return directionToAttacker == facingDirection;
    }

    private bool IsHoldingBack(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) < inputDeadZone)
            return false;

        return Mathf.Sign(horizontalInput) != facingDirection;
    }

    #endregion

    #region Knockdown And Recovery

    private void ApplyGroundedState()
    {
        isKnockedDown = true;
        isRecovering = false;
        groundedTimer = groundedTime;
        recoveryTimer = 0f;

        isBlocking = false;
        isCrouching = false;

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
            groundedPositionX = rb.position.x;
            hasGroundedPositionLock = true;
        }
    }

    private void MaintainGroundedHorizontalPosition()
    {
        if (rb == null || !hasGroundedPositionLock ||
            (!isKnockedDown && !isRecovering))
        {
            return;
        }

        Vector3 position = rb.position;
        position.x = groundedPositionX;
        rb.position = position;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0f;
        rb.linearVelocity = velocity;
    }

    private void ReleaseGroundedHorizontalPosition()
    {
        hasGroundedPositionLock = false;
    }

    private void UpdateGroundedStateTimers()
    {
        if (isKnockedDown)
        {
            groundedTimer -= Time.deltaTime;

            if (groundedTimer <= 0f)
                StartRecovery(false);

            return;
        }

        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;

            if (recoveryTimer <= 0f)
                EndRecovery();
        }
    }

    private void CheckForManualRecovery()
    {
        if (!TryGetCurrentInput(
            out _,
            out _,
            out bool punchPressed,
            out bool kickPressed,
            out _,
            out _,
            out _))
        {
            return;
        }

        if (punchPressed || kickPressed)
        {
            StartRecovery(true);
        }

        ClearAIButtonInputs();
    }

    private void StartRecovery(bool isManualRecovery)
    {
        isKnockedDown = false;
        isRecovering = true;

        recoveryTimer = isManualRecovery
            ? manualRecoveryTime
            : recoveryTime;

        if (fighterAnim != null)
        {
            fighterAnim.SetFloat(
                "recoverySpeed",
                isManualRecovery
                    ? manualRecoveryAnimationSpeed
                    : automaticRecoveryAnimationSpeed
            );
        }
    }

    private void EndRecovery()
    {
        isRecovering = false;
        recoveryTimer = 0f;

        ReleaseGroundedHorizontalPosition();

        if (fighterAnim != null)
            fighterAnim.SetFloat("recoverySpeed", 1f);
    }

    #endregion

    #region State Updates

    private void UpdateFacingDirection()
    {
        if (dreamTraversalMode)
        {
            facingDirection = 1;
            FlipModel(1f);
            return;
        }

        if (opponent == null)
            return;

        facingDirection = opponent.position.x > transform.position.x ? 1 : -1;

        FlipModel(facingDirection);
    }

    private void UpdateGrounded()
    {
        bool wasGrounded = isGrounded;

        bool isRising = rb != null && rb.linearVelocity.y > 0.05f;

        isGrounded = !isRising && Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            );

        if (!wasGrounded && isGrounded)
        {
            if (isAttackAnimationPlaying && attackStartedAirborne)
                EndAttackAnimation();

            if (roundActive)
                PlaySound(jumpLandSound);
        }
    }

    private void UpdateBlockStunTimer()
    {
        if (blockStunTimer > 0f)
            blockStunTimer -= Time.deltaTime;
    }

    #endregion

    #region Animation

    /// <summary>
    /// Updates animation for the fighter based off current state - HG
    /// </summary>
    private void UpdateAnimation()
    {
        fighterAnim.SetBool("shuffling", isShuffling);
        fighterAnim.SetBool("walkNormal", walkNormal);
        fighterAnim.SetBool("crouching", isCrouching);
        fighterAnim.SetBool("blocking", isBlocking);
        fighterAnim.SetBool("jumping", !isGrounded);
        fighterAnim.SetBool("stunned", isKnockedDown);
        fighterAnim.SetBool("recovering", isRecovering);
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        Debug.Log("PlaySound called");

        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return;
        }

        Debug.Log("Playing clip: " + clip.name);
        audioSource.PlayOneShot(clip);
    }

    private void PlayAttackResultSound(FighterMoveResult result, AudioClip hitSound)
    {
        if (result == FighterMoveResult.Hit)
        {
            PlaySound(hitSound);
        }
        else if (result == FighterMoveResult.Blocked)
        {
            PlaySound(blockSound);
        }
        else if (result == FighterMoveResult.Miss)
        {
            PlaySound(attackMissSound);
        }
    }

    #endregion
}