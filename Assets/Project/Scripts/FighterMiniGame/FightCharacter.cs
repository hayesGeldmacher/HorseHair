using UnityEngine;
using TMPro;

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
    [Header("References")]
    [Tooltip("Input component used by the player Leave empty for AI fighters")]
    [SerializeField] private FighterInput input;

    [Header("Animation")]
    [Tooltip("Should this fighter be animated")]
    [SerializeField] private bool animateFighter = true;
    [Tooltip("Animator component used for fighter model")]
    [SerializeField] private Animator fighterAnim;
    [Tooltip("Should fighter shuffle or walk forward")]
    [SerializeField] private bool walkNormal = false;
    [Tooltip("The physical transform of the fighter's 3D model")]
    [SerializeField] private Transform fighterModel;


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

    [Header("Blocking")]
    [Tooltip("Damage dealt when an attack is blocked Chip damage cannot defeat")]
    [SerializeField] private int blockChipDamage = 2;

    [Tooltip("How long the blocking fighter is stunned after successfully blocking an attack")]
    [SerializeField] private float blockStunTime = 0.35f;

    [Tooltip("Small knockback applied after blocking an attack")]
    [SerializeField] private float blockKnockbackForce = 2f;

    [Header("Hit Pushback")]
    [Tooltip("Force applied to the opponent after a normal attack lands")]
    [SerializeField] private float hitPushbackForce = 3f;

    [Header("Action Text")]
    [Tooltip("How long temporary actions stay visible before returning to normal state text")]
    [SerializeField] private float actionTextHoldTime = 0.4f;


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



    private bool isMoving; //for animation script to read player state -HG
    private bool isGrounded;
    private bool isCrouching;
    private bool isBlocking;
    private bool isKnockedDown;
    private bool isRecovering;

    private int facingDirection = 1;
    private float actionTextTimer;
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

    private bool roundActive = true;
    private Transform mainCameraTransform;

    public bool IsRoundActive
    {
        get { return roundActive; }
    }

    public event System.Action<FightCharacter, FighterMoveType, FighterMoveResult> MovePerformed;

    private void Reset()
    {
        AssignMissingReferences();
    }

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
        UpdateGroundedStateTimers();

        if (animateFighter) { UdpateAnimation(); }

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

        if (isKnockedDown)
        {
            SetActionText("Grounded");
            CheckForManualRecovery();
            return;
        }

        if (isRecovering)
        {
            SetActionText("Recovering");
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

    private void AssignMissingReferences()
    {
        rb ??= GetComponent<Rigidbody>();
        input ??= GetComponent<FighterInput>();
        health ??= GetComponent<FighterHealth>();
        audioSource ??= GetComponent<AudioSource>();
    }

    public void SetAIInput(float moveInput, bool jumpPressed, bool punchPressed, bool kickPressed, bool grabPressed, bool crouchHeld)
    {
        aiMoveInput = moveInput;
        aiJumpPressed = jumpPressed;
        aiPunchPressed = punchPressed;
        aiKickPressed = kickPressed;
        aiGrabPressed = grabPressed;
        aiCrouchHeld = crouchHeld;
    }

    private void ReadActions()
    {
        if (!TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool grabPressed, out bool crouchHeld))
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
            SetTemporaryActionText("Jump");
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

        UpdateNormalActionText(moveInput);
        ClearAIButtonInputs();
    }

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

    private void Move()
    {
        isMoving = false;
        if (rb == null || blockStunTimer > 0f || isKnockedDown || isRecovering)
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

        if (!TryGetCurrentInput(out float moveInput, out _, out _, out _, out _, out _))
            return;

        float horizontal = Mathf.Abs(moveInput) < inputDeadZone ? 0f : moveInput;
        float currentMoveSpeed = moveSpeed;
        isMoving = Mathf.Abs(moveInput) < inputDeadZone ? false : true; //check if player is moving or still - HG

        if (isBlocking)
            currentMoveSpeed = blockMoveSpeed;
        else if (isCrouching)
            currentMoveSpeed = crouchMoveSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontal * currentMoveSpeed;
        rb.linearVelocity = velocity;
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

        SetTemporaryActionText(direction == facingDirection ? "Forward Quickstep" : "Back Quickstep");
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

    private bool TryGetCurrentInput(out float moveInput, out bool jumpPressed, out bool punchPressed, out bool kickPressed, out bool grabPressed, out bool crouchHeld)
    {
        if (controlledByAI)
        {
            moveInput = aiMoveInput;
            jumpPressed = aiJumpPressed;
            punchPressed = aiPunchPressed;
            kickPressed = aiKickPressed;
            grabPressed = aiGrabPressed;
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
            crouchHeld = false;
            return false;
        }

        moveInput = input.Move;
        jumpPressed = input.JumpPressed;
        punchPressed = input.PunchPressed;
        kickPressed = input.KickPressed;
        grabPressed = input.GrabPressed;
        crouchHeld = input.CrouchHeld;

        return true;
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

        if (animateFighter) { fighterAnim.SetTrigger("jump"); }
    }

    private void Punch()
    {
        string punchType = GetPunchType();
        FighterMoveType moveType = GetPunchMoveType();

        int damage = GetPunchDamage();
        float range = GetPunchRange();
        AttackHeight attackHeight = GetPunchHeight();

        SetTemporaryActionText(punchType);

        FighterMoveResult result = TryHitOpponent(punchType, damage, range, attackHeight);

        PlayAttackResultSound(result, punchHitSound);

        MovePerformed?.Invoke(this, moveType, result);
        if (animateFighter) { fighterAnim.SetTrigger("punch"); }
    }

    private void Kick()
    {
        string kickType = GetKickType();
        FighterMoveType moveType = GetKickMoveType();

        int damage = GetKickDamage();
        float range = GetKickRange();
        AttackHeight attackHeight = GetKickHeight();

        SetTemporaryActionText(kickType);

        FighterMoveResult result = TryHitOpponent(kickType, damage, range, attackHeight);

        PlayAttackResultSound(result, kickHitSound);

        MovePerformed?.Invoke(this, moveType, result);

        if (animateFighter) { fighterAnim.SetTrigger("kick"); }
    }

    private void Grab()
    {
        if (!isGrounded || isCrouching || isBlocking || isKnockedDown || isRecovering)
        {
            PlaySound(attackMissSound);
            SetTemporaryActionText("Grab Failed");
            MovePerformed?.Invoke(this, FighterMoveType.Grab, FighterMoveResult.Miss);
            return;
        }

        SetTemporaryActionText("Grab");

        FighterMoveResult result = TryGrabOpponent();

        if (result == FighterMoveResult.Hit)
            PlaySound(grabSound);
        else
            PlaySound(attackMissSound);

        MovePerformed?.Invoke(this, FighterMoveType.Grab, result);
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
    public FighterMoveResult ReceiveGrab(FightCharacter attacker, int damage)
    {
        if (isKnockedDown || isRecovering)
            return FighterMoveResult.Miss;

        if (quickstepTimer > 0f)
            return FighterMoveResult.Miss;

        if (attacker == null)
            return FighterMoveResult.Miss;

        if (health != null)
            health.TakeDamage(damage, true);

        SwitchSidesWithAttacker(attacker);
        ApplyGroundedState();

        SetTemporaryActionText("Grabbed");

        return FighterMoveResult.Hit;
    }

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

    public void FlipModel(float direction)
    {
        //flips the fighter model on grab - HG
        if (fighterModel != null)
        {
            Vector3 scale = fighterModel.localScale;
            scale.x = Mathf.Abs(scale.x); //re-orient
            scale.x *= direction;
            if (scale.x != fighterModel.localScale.x) { fighterModel.localScale = scale; }
        }
    }

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

    private float GetPunchRange()
    {
        if (!isGrounded)
            return jumpingPunchRange;

        if (isCrouching)
            return crouchingPunchRange;

        return standingPunchRange;
    }

    private int GetKickDamage()
    {
        if (!isGrounded)
            return jumpingKickDamage;

        if (isCrouching)
            return crouchingKickDamage;

        return standingKickDamage;
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

        return opponentCharacter.ReceiveAttack(damage, transform.position, attackHeight);
    }

    /// <summary>
    /// If blocking correctly, applies chip damage, block stun, and small knockback.
    /// If not blocking, applies full damage.
    /// </summary>
    public FighterMoveResult ReceiveAttack(int damage, Vector3 attackerPosition, AttackHeight attackHeight)
    {
        if (isKnockedDown)
            return FighterMoveResult.Miss;

        if (quickstepTimer > 0f)
            return FighterMoveResult.Miss;

        if (IsBlockingAttack(attackerPosition, attackHeight))
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
        ApplyHitPushback(attackerPosition);
        SetTemporaryActionText("Hit");

        return FighterMoveResult.Hit;
    }

    /// <summary>
    /// Chip damage cannot defeat the fighter.
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
        velocity.x = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.right * directionAwayFromAttacker * blockKnockbackForce, ForceMode.Impulse);
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

    private void UpdateBlockStunTimer()
    {
        if (blockStunTimer > 0f)
            blockStunTimer -= Time.deltaTime;
    }

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
                StartRecovery();

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
        if (!TryGetCurrentInput(out _, out _, out bool punchPressed, out bool kickPressed, out _, out _))
            return;

        if (punchPressed || kickPressed)
            StartRecovery();

        ClearAIButtonInputs();
    }

    private void StartRecovery()
    {
        isKnockedDown = false;
        isRecovering = true;
        recoveryTimer = recoveryTime;
    }

    private void EndRecovery()
    {
        isRecovering = false;
        recoveryTimer = 0f;
    }

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

    private void SetTemporaryActionText(string action)
    {
        SetActionText(action);
        actionTextTimer = actionTextHoldTime;
    }

    private void UpdateActionTextTimer()
    {
        if (actionTextTimer > 0f)
            actionTextTimer -= Time.deltaTime;
    }

    private void SetActionText(string action)
    {
        if (actionText == null)
            return;

        actionText.text = action;
    }

    private void FaceActionTextToCamera()
    {
        if (actionText == null || mainCameraTransform == null)
            return;

        actionText.transform.rotation = Quaternion.LookRotation(
            actionText.transform.position - mainCameraTransform.position
        );
    }

    private bool IsBlockingAttack(Vector3 attackerPosition, AttackHeight attackHeight)
    {
        return isBlocking
            && IsAttackFromFront(attackerPosition)
            && IsBlockingCorrectHeight(attackHeight);
    }

    /// <summary>
    /// Standing block blocks high attacks, Crouching block blocks low attacks
    /// </summary>
    private bool IsBlockingCorrectHeight(AttackHeight attackHeight)
    {
        if (isCrouching)
            return attackHeight == AttackHeight.Low;

        return attackHeight == AttackHeight.High;
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

    private void UpdateFacingDirection()
    {
        if (opponent == null)
            return;

        facingDirection = opponent.position.x > transform.position.x ? 1 : -1;
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

    /// <summary>
    /// Updates animation for the fighter based off current state - HG
    /// </summary>
    private void UdpateAnimation()
    {
        fighterAnim.SetBool("moving", isMoving);
        fighterAnim.SetBool("walkNormal", walkNormal);
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
    }
}