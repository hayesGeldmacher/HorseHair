using UnityEngine;
using TMPro;

/// <summary>
/// Controls fighter movement, combat actions, defensive states, AI input, and action text display
/// This script is used by both the player and the enemy
/// </summary>
public class FightCharacter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Input component used by the player Leave empty for AI fighters")]
    [SerializeField] private FighterInput input;

    [Tooltip("Opponent this fighter faces and attacks")]
    [SerializeField] private Transform opponent;

    [Tooltip("Rigidbody used for movement and jumping")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("Health component used when this fighter takes damage")]
    [SerializeField] private FighterHealth health;

    [Tooltip("Text label used to display the fighter's current action")]
    [SerializeField] private TMP_Text actionText;

    [Header("Control Type")]
    [Tooltip("Turn this on for AI-controlled fighters Leave it off for the player")]
    [SerializeField] private bool controlledByAI;

    [Header("Movement")]
    [Tooltip("Horizontal movement speed")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Horizontal movement speed while holding back to block")]
    [SerializeField] private float blockMoveSpeed = 2.5f;

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

    [Header("Combat")]
    [Tooltip("Damage dealt by a successful punch")]
    [SerializeField] private int punchDamage = 10;

    [Tooltip("Maximum distance required for a punch to hit")]
    [SerializeField] private float punchRange = 1.5f;

    [Tooltip("Damage dealt by a successful kick")]
    [SerializeField] private int kickDamage = 16;

    [Tooltip("Maximum distance required for a kick to hit")]
    [SerializeField] private float kickRange = 2f;

    [Tooltip("Damage dealt when an attack is blocked Chip damage cannot defeat")]
    [SerializeField] private int blockChipDamage = 2;

    [Tooltip("How long the blocking fighter is stunned after successfully blocking an attack")]
    [SerializeField] private float blockStunTime = 0.35f;

    [Tooltip("Small knockback applied after blocking an attack")]
    [SerializeField] private float blockKnockbackForce = 2f;

    [Header("Action Text")]
    [Tooltip("How long temporary actions stay visible before returning to normal state text")]
    [SerializeField] private float actionTextHoldTime = 0.4f;

    private bool isGrounded;
    private bool isCrouching;
    private bool isBlocking;

    private int facingDirection = 1;
    private float actionTextTimer;
    private float blockStunTimer;

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

    private bool roundActive = true;
    private Transform mainCameraTransform;

    public bool IsRoundActive
    {
        get { return roundActive; }
    }

    public event System.Action<FightCharacter, FighterMoveType, FighterMoveResult> MovePerformed;

    private void Reset()
    {
        AssignDefaultReferences();
    }

    private void Awake()
    {
        AssignMissingReferences();

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        UpdateGrounded();
        UpdateFacingDirection();
        UpdateActionTextTimer();
        UpdateBlockStunTimer();
        UpdateQuickstepTimers();

        if (!roundActive)
        {
            SetActionText("Round Over");
            return;
        }

        if (blockStunTimer > 0f)
        {
            SetActionText("Block Stun");
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

    private void LateUpdate()
    {
        FaceActionTextToCamera();
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
        rb ??= GetComponent<Rigidbody>();
        input ??= GetComponent<FighterInput>();
        health ??= GetComponent<FighterHealth>();
    }

    /// <summary>
    /// Allows FightCharacterAI to send movement, jump, and punch commands into this fighter
    /// This lets AI fighters use the same movement and combat logic as the player
    /// </summary>
    public void SetAIInput(float moveInput, bool jumpPressed, bool punchPressed, bool kickPressed, bool crouchHeld)
    {
        aiMoveInput = moveInput;
        aiJumpPressed = jumpPressed;
        aiPunchPressed = punchPressed;
        aiKickPressed = kickPressed;
        aiCrouchHeld = crouchHeld;
    }

    /// <summary>
    /// Reads either player input or AI input and updates the fighter's current action
    /// </summary>
    private void ReadActions()
    {
        if (!TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool crouchHeld))
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
            Jump();
            SetTemporaryActionText("Jump");
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

        UpdateNormalActionText(moveInput);
        ClearAIButtonInputs();
    }

    /// <summary>
    /// Enables or disables this fighter during round transitions
    /// </summary>
    public void SetRoundActive(bool isActive)
    {
        roundActive = isActive;

        if (!roundActive && rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// Moves the fighter horizontally based on player input or AI input
    /// Blocking movement is slower than normal movement
    /// Crouching prevents horizontal movement
    /// Block stun prevents movement
    /// </summary>
    private void Move()
    {
        if (rb == null || blockStunTimer > 0f)
            return;

        if (quickstepTimer > 0f)
        {
            Vector3 quickstepVelocity = rb.linearVelocity;
            quickstepVelocity.x = quickstepDirection * quickstepSpeed;
            rb.linearVelocity = quickstepVelocity;
            return;
        }

        if (!TryGetCurrentInput(out float moveInput, out _, out _, out _, out _))
            return;

        float horizontal = Mathf.Abs(moveInput) < inputDeadZone ? 0f : moveInput;

        if (isCrouching)
            horizontal = 0f;

        float currentMoveSpeed = isBlocking ? blockMoveSpeed : moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontal * currentMoveSpeed;
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Detects double taps in the same direction to trigger a quickstep dodge
    /// </summary>
    private void CheckQuickstepInput(float moveInput)
    {
        if (!isGrounded || isCrouching || blockStunTimer > 0f)
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

    /// <summary>
    /// Initiates a quickstep dodge in the specified direction, setting timers for the quickstep duration and cooldown
    /// </summary>
    private void StartQuickstep(float direction)
    {
        quickstepDirection = direction;
        quickstepTimer = quickstepTime;
        quickstepCooldownTimer = quickstepCooldown;

        isBlocking = false;
        isCrouching = false;

        SetTemporaryActionText(direction == facingDirection ? "Forward Quickstep" : "Back Quickstep");
    }

    /// <summary>
    /// Applies quickstep movement for a short duration and counts down the quickstep and cooldown timers
    /// </summary>
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

    /// <summary>
    /// Determines the current input values from either the player input component or the AI input variables
    /// </summary>
    /// <returns></returns>
    private bool TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool crouchHeld)
    {
        if (controlledByAI)
        {
            moveInput = aiMoveInput;
            jumpPressed = aiJumpPressed;
            punchPressed = aiPunchPressed;
            kickPressed = aiKickPressed;
            crouchHeld = aiCrouchHeld;
            return true;
        }

        if (input == null)
        {
            moveInput = 0f;
            jumpPressed = false;
            punchPressed = false;
            kickPressed = false;
            crouchHeld = false;
            return false;
        }

        moveInput = input.Move;
        jumpPressed = input.JumpPressed;
        punchPressed = input.PunchPressed;
        kickPressed = input.KickPressed;
        crouchHeld = input.CrouchHeld;
        return true;
    }

    /// <summary>
    /// Applies upward force to make the fighter jump
    /// </summary>
    private void Jump()
    {
        if (rb == null)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Determines the current punch type and attempts to hit the opponent
    /// </summary>
    private void Punch()
    {
        string punchType = GetPunchType();
        FighterMoveType moveType = GetMoveType();

        SetTemporaryActionText(punchType);

        FighterMoveResult result = TryHitOpponent(punchType, punchDamage, punchRange);

        MovePerformed?.Invoke(this, moveType, result);
    }

    /// <summary>
    /// Determines the current kick type and attempts to hit the opponent
    /// </summary>
    private void Kick()
    {
        string kickType = GetKickType();
        FighterMoveType moveType = GetKickMoveType();

        SetTemporaryActionText(kickType);

        FighterMoveResult result = TryHitOpponent(kickType, kickDamage, kickRange);

        MovePerformed?.Invoke(this, moveType, result);
    }

    private FighterMoveType GetKickMoveType()
    {
        if (!isGrounded)
            return FighterMoveType.JumpingKick;

        if (isCrouching)
            return FighterMoveType.CrouchingKick;

        return FighterMoveType.StandingKick;
    }

    private string GetKickType()
    {
        if (!isGrounded)
            return "Jumping Kick";

        if (isCrouching)
            return "Crouching Kick";

        return "Standing Kick";
    }

    /// <summary>
    /// Determines the type of move based on the fighter's current state
    /// </summary>
    private FighterMoveType GetMoveType()
    {
        if (!isGrounded)
            return FighterMoveType.JumpingPunch;

        if (isCrouching)
            return FighterMoveType.CrouchingPunch;

        return FighterMoveType.StandingPunch;
    }

    /// <summary>
    /// Determines the type of punch based on the fighter's current state
    /// </summary>
    private string GetPunchType()
    {
        if (!isGrounded)
            return "Jumping Punch";

        if (isCrouching)
            return "Crouching Punch";

        return "Standing Punch";
    }

    /// <summary>
    /// Checks whether the opponent is in range and sends the attack to the opponent's FightCharacter
    /// This allows blocking, chip damage, block stun, and knockback to work
    /// </summary>
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

        return opponentCharacter.ReceiveAttack(damage, transform.position);
    }

    /// <summary>
    /// Receives incoming damage
    /// If blocking correctly, applies chip damage, block stun, and small knockback
    /// If not blocking, applies full damage
    /// </summary>
    public FighterMoveResult ReceiveAttack(int damage, Vector3 attackerPosition)
    {
        if (quickstepTimer > 0f)
        {
            return FighterMoveResult.Miss;
        }

        if (IsBlockingAttack(attackerPosition))
        {
            ApplyBlockedAttack(attackerPosition);
            return FighterMoveResult.Blocked;
        }

        if (health == null)
        {
            Debug.Log(name + " has no FighterHealth script");
            return FighterMoveResult.Miss;
        }

        health.TakeDamage(damage, true);
        SetTemporaryActionText("Hit");

        return FighterMoveResult.Hit;
    }

    /// <summary>
    /// Handles what happens when this fighter blocks an attack
    /// Blocked attacks deal chip damage, cause block stun, and push the fighter back slightly
    /// Chip damage cannot defeat the fighter
    /// </summary>
    private void ApplyBlockedAttack(Vector3 attackerPosition)
    {
        if (health != null)
            health.TakeDamage(blockChipDamage, false);

        ApplyBlockStun();
        ApplyBlockKnockback(attackerPosition);

        SetTemporaryActionText("Blocked");
        Debug.Log(name + " blocked the attack and took chip damage.");
    }

    /// <summary>
    /// Prevents the fighter from acting briefly after blocking
    /// </summary>
    private void ApplyBlockStun()
    {
        blockStunTimer = blockStunTime;
    }

    /// <summary>
    /// Pushes the fighter slightly away from the attacker after blocking
    /// </summary>
    private void ApplyBlockKnockback(Vector3 attackerPosition)
    {
        if (rb == null)
            return;

        float directionAwayFromAttacker = Mathf.Sign(transform.position.x - attackerPosition.x);

        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.right * directionAwayFromAttacker * blockKnockbackForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Counts down the block stun timer
    /// </summary>
    private void UpdateBlockStunTimer()
    {
        if (blockStunTimer > 0f)
            blockStunTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Updates the action text for normal movement and defensive states
    /// </summary>
    private void UpdateNormalActionText(float moveInput)
    {
        if (actionTextTimer > 0f)
            return;

        if (isBlocking && isCrouching)
        {
            SetActionText("Crouching Block");
        }
        else if (isBlocking)
        {
            SetActionText("Standing Block");
        }
        else if (isCrouching)
        {
            SetActionText("Crouch");
        }
        else if (!isGrounded)
        {
            SetActionText("Jumping");
        }
        else if (Mathf.Abs(moveInput) > inputDeadZone)
        {
            SetActionText("Walking");
        }
        else
        {
            SetActionText("Idle");
        }
    }

    /// <summary>
    /// Displays an action for a limited amount of time
    /// </summary>
    private void SetTemporaryActionText(string action)
    {
        SetActionText(action);
        actionTextTimer = actionTextHoldTime;
    }

    /// <summary>
    /// Counts down the temporary action text timer
    /// </summary>
    private void UpdateActionTextTimer()
    {
        if (actionTextTimer > 0f)
            actionTextTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Updates the action text label if one is assigned
    /// </summary>
    private void SetActionText(string action)
    {
        if (actionText == null)
            return;

        actionText.text = action;
    }

    /// <summary>
    /// Rotates the action text so it faces the main camera
    /// </summary>
    private void FaceActionTextToCamera()
    {
        if (actionText == null || mainCameraTransform == null)
            return;

        actionText.transform.rotation = Quaternion.LookRotation(
            actionText.transform.position - mainCameraTransform.position
        );
    }

    /// <summary>
    /// Returns true when the fighter is blocking and the attack comes from the front
    /// </summary>
    private bool IsBlockingAttack(Vector3 attackerPosition)
    {
        return isBlocking && IsAttackFromFront(attackerPosition);
    }

    /// <summary>
    /// Checks whether the attacker is positioned in front of this fighter
    /// </summary>
    private bool IsAttackFromFront(Vector3 attackerPosition)
    {
        float directionToAttacker = Mathf.Sign(attackerPosition.x - transform.position.x);
        return directionToAttacker == facingDirection;
    }

    /// <summary>
    /// Returns true when the fighter is holding away from the opponent
    /// </summary>
    private bool IsHoldingBack(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) < inputDeadZone)
            return false;

        return Mathf.Sign(horizontalInput) != facingDirection;
    }

    /// <summary>
    /// Updates the direction this fighter is facing based on the opponent's position
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (opponent == null)
            return;

        facingDirection = opponent.position.x > transform.position.x ? 1 : -1;
    }

    /// <summary>
    /// Checks whether the fighter is standing on the ground
    /// </summary>
    private void UpdateGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    /// <summary>
    /// Clears one-frame AI button inputs after they are read
    /// Movement input is not cleared because the AI may need to keep holding a direction
    /// </summary>
    private void ClearAIButtonInputs()
    {
        if (!controlledByAI)
            return;

        aiJumpPressed = false;
        aiPunchPressed = false;
        aiKickPressed = false;
    }
}