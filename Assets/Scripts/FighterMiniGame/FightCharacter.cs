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

    private Vector2 aiMoveInput;
    private bool aiJumpPressed;
    private bool aiPunchPressed;

    private bool roundActive = true;

    private void Reset()
    {
        AssignDefaultReferences();
    }

    private void Awake()
    {
        AssignMissingReferences();
    }

    private void Update()
    {
        UpdateGrounded();
        UpdateFacingDirection();
        UpdateActionTextTimer();
        UpdateBlockStunTimer();

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
        rb = GetComponent<Rigidbody>();
        input = GetComponent<FighterInput>();
        health = GetComponent<FighterHealth>();
    }

    /// <summary>
    /// Assigns required references at runtime if they were not assigned in the Inspector
    /// </summary>
    private void AssignMissingReferences()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (input == null)
            input = GetComponent<FighterInput>();

        if (health == null)
            health = GetComponent<FighterHealth>();
    }

    /// <summary>
    /// Allows FightCharacterAI to send movement, jump, and punch commands into this fighter
    /// This lets AI fighters use the same movement and combat logic as the player
    /// </summary>
    public void SetAIInput(Vector2 moveInput, bool jumpPressed, bool punchPressed)
    {
        aiMoveInput = moveInput;
        aiJumpPressed = jumpPressed;
        aiPunchPressed = punchPressed;
    }

    /// <summary>
    /// Reads either player input or AI input and updates the fighter's current action
    /// </summary>
    private void ReadActions()
    {
        if (!controlledByAI && input == null)
            return;

        Vector2 moveInput = controlledByAI ? aiMoveInput : input.Move;
        bool jumpPressed = controlledByAI ? aiJumpPressed : input.JumpPressed;
        bool punchPressed = controlledByAI ? aiPunchPressed : input.PunchPressed;

        bool holdingDown = moveInput.y < -inputDeadZone;
        bool holdingBack = IsHoldingBack(moveInput.x);

        isCrouching = holdingDown && isGrounded;
        isBlocking = holdingBack && isGrounded;

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
        if (rb == null)
            return;

        if (blockStunTimer > 0f)
            return;

        if (!controlledByAI && input == null)
            return;

        Vector2 moveInput = controlledByAI ? aiMoveInput : input.Move;
        float horizontal = moveInput.x;

        if (Mathf.Abs(horizontal) < inputDeadZone)
            horizontal = 0f;

        if (isCrouching)
            horizontal = 0f;

        float currentMoveSpeed = isBlocking ? blockMoveSpeed : moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontal * currentMoveSpeed;
        rb.linearVelocity = velocity;
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
        string punchType;

        if (!isGrounded)
        {
            punchType = "Jumping Punch";
        }
        else if (isCrouching)
        {
            punchType = "Crouching Punch";
        }
        else
        {
            punchType = "Standing Punch";
        }

        SetTemporaryActionText(punchType);
        TryHitOpponent(punchType);
    }

    /// <summary>
    /// Checks whether the opponent is in range and sends the attack to the opponent's FightCharacter
    /// This allows blocking, chip damage, block stun, and knockback to work
    /// </summary>
    private void TryHitOpponent(string punchType)
    {
        if (opponent == null)
        {
            Debug.Log("No opponent assigned");
            return;
        }

        float distanceToOpponent = Vector3.Distance(transform.position, opponent.position);

        if (distanceToOpponent > punchRange)
        {
            Debug.Log(punchType + " missed");
            return;
        }

        FightCharacter opponentCharacter = opponent.GetComponent<FightCharacter>();

        if (opponentCharacter == null)
        {
            Debug.Log("Opponent has no FightCharacter script.");
            return;
        }

        opponentCharacter.ReceiveAttack(punchDamage, transform.position);
    }

    /// <summary>
    /// Receives incoming damage
    /// If blocking correctly, applies chip damage, block stun, and small knockback
    /// If not blocking, applies full damage
    /// </summary>
    public void ReceiveAttack(int damage, Vector3 attackerPosition)
    {
        if (IsBlockingAttack(attackerPosition))
        {
            ApplyBlockedAttack(attackerPosition);
            return;
        }

        if (health == null)
        {
            Debug.Log(name + " has no FighterHealth script");
            return;
        }

        health.TakeDamage(damage, true);
        SetTemporaryActionText("Hit");
    }

    /// <summary>
    /// Handles what happens when this fighter blocks an attack
    /// Blocked attacks deal chip damage, cause block stun, and push the fighter back slightly
    /// Chip damage cannot defeat the fighter
    /// </summary>
    private void ApplyBlockedAttack(Vector3 attackerPosition)
    {
        if (health != null)
        {
            health.TakeDamage(blockChipDamage, false);
        }

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
        {
            blockStunTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Updates the action text for normal movement and defensive states
    /// </summary>
    private void UpdateNormalActionText(Vector2 moveInput)
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
        else if (Mathf.Abs(moveInput.x) > inputDeadZone)
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
        if (actionText == null || Camera.main == null)
            return;

        actionText.transform.rotation = Quaternion.LookRotation(
            actionText.transform.position - Camera.main.transform.position
        );
    }

    /// <summary>
    /// Returns true when the fighter is blocking and the attack comes from the front
    /// </summary>
    private bool IsBlockingAttack(Vector3 attackerPosition)
    {
        if (!isBlocking)
            return false;

        return IsAttackFromFront(attackerPosition);
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

        if (opponent.position.x > transform.position.x)
        {
            facingDirection = 1;
        }
        else
        {
            facingDirection = -1;
        }
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
    }
}