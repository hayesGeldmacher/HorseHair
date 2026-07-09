using UnityEngine;

/// <summary>
/// Controls fighter movement, combat actions, defensive states, AI input, and action text display
/// This script is used by both the player and the enemy
/// </summary>
public enum AttackHeight
{
    High,
    Low
}

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

    [Header("Control Type")]
    [Tooltip("Turn this on for AI-controlled fighters Leave it off for the player")]
    [SerializeField] private bool controlledByAI;

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

    [Tooltip("Height of the special attack, either High or Low")]
    [SerializeField] private AttackHeight specialAttackHeight = AttackHeight.High;

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

    [Header("Fighter Body Resistance")]
    [Tooltip("Minimum horizontal distance maintained between fighters")]
    [SerializeField] private float minimumFighterDistance = 1.1f;

    [Tooltip("How much resistance there is when fighters are touching. 0 prevents pushing; 0.1 allows slight pushing")]
    [Range(0f, 1f)]
    [SerializeField] private float bodyPushAmount = 0.1f;

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
    private AttackHeight pendingAttackHeight;
    private AudioClip pendingHitSound;
    private bool hasPendingAttack;

    private bool hasPendingGrab;

    private bool isAttackAnimationPlaying; //prevents attack spam while an attack animation is still playing 

    private bool roundActive = true;

    #endregion

    #region Public API

    public bool IsRoundActive
    {
        get { return roundActive; }
    }

    public event System.Action<FightCharacter, FighterMoveType, FighterMoveResult> MovePerformed;

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

    private void ResetRoundState()
    {
        // Movement states
        isShuffling = false;
        isCrouching = false;
        isBlocking = false;

        // Attack states
        isAttackAnimationPlaying = false;
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

        // Reset Animator parameters
        if (fighterAnim != null)
        {
            fighterAnim.ResetTrigger("punch");
            fighterAnim.ResetTrigger("kick");
            fighterAnim.ResetTrigger("grab");
            fighterAnim.ResetTrigger("special");
            fighterAnim.ResetTrigger("jump");
            fighterAnim.ResetTrigger("quickStep");
            fighterAnim.ResetTrigger("hurt");

            fighterAnim.SetBool("shuffling", false);
            fighterAnim.SetBool("crouching", false);
            fighterAnim.SetBool("blocking", false);
            fighterAnim.SetBool("jumping", false);
            fighterAnim.SetBool("stunned", false);
            fighterAnim.SetBool("recovering", false);
            fighterAnim.SetFloat("recoverySpeed", 1f);

            fighterAnim.CrossFade("FighterMovement.Standing.Fighter_Idle_Standing 0", 0.1f);
        }
    }

    public void StartAttackAnimation()
    {
        isAttackAnimationPlaying = true; //locks attack input during attack animation 

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

        if (fighterAnim != null)
            fighterAnim.speed = 1f;

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
        if (!hasPendingAttack)
            return;

        hasPendingAttack = false;

        FighterMoveResult result = TryHitOpponent(
            pendingAttackName,
            pendingAttackDamage,
            pendingAttackRange,
            pendingAttackHeight
        );

        PlayAttackResultSound(result, pendingHitSound);

        MovePerformed?.Invoke(this, pendingMoveType, result);
    }

    public void PerformGrabHit()
    {
        if (!hasPendingGrab)
            return;

        hasPendingGrab = false;

        FighterMoveResult result = TryGrabOpponent();

        if (result == FighterMoveResult.Hit)
            PlaySound(grabSound);
        else
            PlaySound(attackMissSound);

        MovePerformed?.Invoke(this, FighterMoveType.Grab, result);
    }

    public FighterMoveResult ReceiveGrab(FightCharacter attacker, int damage)
    {
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

        health.TakeDamage(damage, true);

        SwitchSidesWithAttacker(attacker);
        ApplyGroundedState();

        return FighterMoveResult.Hit;
    }

    /// <summary>
    /// If blocking correctly, applies chip damage, block stun, and small knockback.
    /// If not blocking, applies full damage.
    /// </summary>
    public FighterMoveResult ReceiveAttack(int damage, Vector3 attackerPosition, AttackHeight attackHeight, FightCharacter attacker)
    {
        if (isKnockedDown)
            return FighterMoveResult.Miss;

        if (quickstepTimer > 0f)
            return FighterMoveResult.Miss;

        if (IsBlockingAttack(attackerPosition))
        {
            ApplyBlockedAttack(attackerPosition, attacker);
            return FighterMoveResult.Blocked;
        }

        if (health == null)
        {
            Debug.Log(name + " has no FighterHealth script");
            return FighterMoveResult.Miss;
        }

        health.TakeDamage(damage, true);
        ApplyDamage();
        ApplyHitPushback(attackerPosition);

        return FighterMoveResult.Hit;
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

    private void Update()
    {
        UpdateGrounded();
        UpdateFacingDirection();

        if (!roundActive)
        {
            return;
        }

        UpdateBlockStunTimer();
        UpdateQuickstepTimers();
        UpdateGroundedStateTimers();

        if (animateFighter && fighterAnim != null)
        {
            UpdateAnimation();
        }

        if (blockStunTimer > 0f)
        {
            return;
        }

        if (isKnockedDown)
        {
            CheckForManualRecovery();
            return;
        }

        if (isRecovering)
        {
            return;
        }

        ReadActions();
    }


    private void FixedUpdate()
    {
        if (!roundActive)
            return;

        Move();
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
    }

    #endregion

    #region Input Reading

    private void ReadActions()
    {
        if (!TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool grabPressed, out bool specialPressed, out bool crouchHeld))
            return;

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

        horizontal = ApplyOpponentBodyResistance(horizontal);

        horizontal = ApplyOpponentBodyResistance(horizontal);

        float currentMoveSpeed = moveSpeed;

        if (!isBlocking) //only shuffling is character is moving forward -HG
        {
            isShuffling = Mathf.Abs(moveInput) < inputDeadZone ? false : true; //check if player is moving or still - HG
        }

        if (isBlocking)
            currentMoveSpeed = blockMoveSpeed;
        else if (isCrouching)
            currentMoveSpeed = crouchMoveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontal * currentMoveSpeed;
        rb.linearVelocity = velocity;
    }

    private float ApplyOpponentBodyResistance(float horizontalInput)
    {
        if (opponent == null)
            return horizontalInput;

        if (Mathf.Abs(horizontalInput) < inputDeadZone)
            return 0f;

        float differenceX = opponent.position.x - transform.position.x;
        float distanceToOpponent = Mathf.Abs(differenceX);

        if (distanceToOpponent <= Mathf.Epsilon)
            return 0f;

        bool movingTowardOpponent = Mathf.Sign(horizontalInput) == Mathf.Sign(differenceX);

        if (movingTowardOpponent && distanceToOpponent <= minimumFighterDistance)
        {
            return horizontalInput * bodyPushAmount;
        }

        return horizontalInput;
    }

    private void Jump(float moveInput)
    {
        PlaySound(jumpSound);

        if (rb == null)
            return;

        float jumpHorizontalDirection = Mathf.Abs(moveInput) < inputDeadZone ? 0f : Mathf.Sign(moveInput);

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        velocity.x = jumpHorizontalDirection * moveSpeed;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animateFighter)
        {
            fighterAnim.SetTrigger("jump");
        }
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
        AttackHeight attackHeight = GetPunchHeight();

        StorePendingAttack(
            punchType,
            moveType,
            damage,
            range,
            attackHeight,
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
        AttackHeight attackHeight = GetKickHeight();

        StorePendingAttack(
            kickType,
            moveType,
            damage,
            range,
            attackHeight,
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

        if (!superMeter.TrySpendSpecial())
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
            specialAttackHeight,
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
        return !isAttackAnimationPlaying && !isKnockedDown && !isRecovering && blockStunTimer <= 0f; //checks if fighter can start an attack 
    }

    private void StorePendingAttack(
        string attackName,
        FighterMoveType moveType,
        int damage,
        float range,
        AttackHeight attackHeight,
        AudioClip hitSound)
    {
        pendingAttackName = attackName;
        pendingMoveType = moveType;
        pendingAttackDamage = damage;
        pendingAttackRange = range;
        pendingAttackHeight = attackHeight;
        pendingHitSound = hitSound;
        hasPendingAttack = true;
    }

    #endregion

    #region Combat Resolution

    private FighterMoveResult TryHitOpponent(string attackName, int damage, float range, AttackHeight attackHeight)
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

        return opponentCharacter.ReceiveAttack(damage, transform.position, attackHeight, this);
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

    /// <summary>
    /// Grabs ignore block, deal damage, switch the fighters' positions, and put this fighter into grounded state.
    /// </summary>
    private void SwitchSidesWithAttacker(FightCharacter attacker)
    {
        Vector3 attackerPosition = attacker.transform.position;
        Vector3 defenderPosition = transform.position;

        float directionFromAttackerToDefender = Mathf.Sign(defenderPosition.x - attackerPosition.x);

        attacker.transform.position = new Vector3(
            defenderPosition.x,
            attackerPosition.y,
            attackerPosition.z
        );

        transform.position = new Vector3(
            attackerPosition.x + directionFromAttackerToDefender * grabSideSwitchOffset,
            defenderPosition.y,
            defenderPosition.z
        );

        if (attacker.rb != null)
        {
            Vector3 attackerVelocity = attacker.rb.linearVelocity;
            attackerVelocity.x = 0f;
            attacker.rb.linearVelocity = attackerVelocity;
        }

        if (rb != null)
        {
            Vector3 defenderVelocity = rb.linearVelocity;
            defenderVelocity.x = 0f;
            rb.linearVelocity = defenderVelocity;
        }

        float newDirection = Mathf.Sign(transform.position.x - attacker.transform.position.x);
        FlipModel(newDirection);
        attacker.FlipModel(-newDirection);
    }

    /// <summary>
    /// Chip damage cannot defeat the fighter.
    /// </summary>
    private void ApplyBlockedAttack(Vector3 attackerPosition, FightCharacter attacker)
    {
        if (health != null)
            health.TakeDamage(blockChipDamage, false);

        ApplyBlockStun();
        ApplyBlockKnockback(attackerPosition);

        if (attacker != null)
            attacker.ApplyBlockRecoil(transform.position);

        Debug.Log(name + " blocked the attack and took chip damage.");
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

    private AttackHeight GetPunchHeight()
    {
        if (isCrouching && isGrounded)
            return AttackHeight.Low;

        return AttackHeight.High;
    }

    private AttackHeight GetKickHeight()
    {
        if (isCrouching && isGrounded)
            return AttackHeight.Low;

        return AttackHeight.High;
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
        }
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

        if (fighterAnim != null)
            fighterAnim.SetFloat("recoverySpeed", 1f);
    }

    #endregion

    #region State Updates

    private void UpdateFacingDirection()
    {
        if (opponent == null)
            return;

        facingDirection = opponent.position.x > transform.position.x ? 1 : -1;

        FlipModel(facingDirection);
    }

    private void UpdateGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );

        if (!wasGrounded && isGrounded && roundActive)
            PlaySound(jumpLandSound);
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