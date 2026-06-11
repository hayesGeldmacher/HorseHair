using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads player input for a fighter character and exposes the current input state
/// to other scripts, such as FightCharacter
/// </summary>
public class FighterInput : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Reads horizontal movement input")]
    [SerializeField] private InputActionReference moveAction;

    [Tooltip("Triggered when the player presses the jump input")]
    [SerializeField] private InputActionReference jumpAction;

    [Tooltip("Triggered when the player presses the punch input")]
    [SerializeField] private InputActionReference punchAction;

    [Tooltip("Held when the player holds the crouch input")]
    [SerializeField] private InputActionReference crouchAction;

    /// <summary>
    /// Current movement input
    /// X controls left and right movement
    /// </summary>
    public float Move { get; private set; }

    /// <summary>
    /// True for one frame when the jump input is pressed
    /// </summary>
    public bool JumpPressed { get; private set; }

    /// <summary>
    /// True for one frame when the punch input is pressed
    /// </summary>
    public bool PunchPressed { get; private set; }

    /// <summary>
    /// True while the crouch input is held
    /// </summary>
    public bool CrouchHeld { get; private set; }

    private void OnEnable()
    {
        EnableInputActions();
        SubscribeToInputEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
        DisableInputActions();
    }

    private void Update()
    {
        ReadMovementInput();
        ReadCrouchInput();
    }

    private void LateUpdate()
    {
        ResetOneFrameInputs();
    }

    /// <summary>
    /// Enables all assigned input actions so they can receive input
    /// </summary>
    private void EnableInputActions()
    {
        if (moveAction != null)
            moveAction.action.Enable();

        if (jumpAction != null)
            jumpAction.action.Enable();

        if (punchAction != null)
            punchAction.action.Enable();

        if (crouchAction != null)
            crouchAction.action.Enable();
    }

    /// <summary>
    /// Disables all assigned input actions when this component is inactive
    /// </summary>
    private void DisableInputActions()
    {
        if (moveAction != null)
            moveAction.action.Disable();

        if (jumpAction != null)
            jumpAction.action.Disable();

        if (punchAction != null)
            punchAction.action.Disable();

        if (crouchAction != null)
            crouchAction.action.Disable();
    }

    /// <summary>
    /// Subscribes to button input events
    /// </summary>
    private void SubscribeToInputEvents()
    {
        if (jumpAction != null)
            jumpAction.action.performed += OnJump;

        if (punchAction != null)
            punchAction.action.performed += OnPunch;
    }

    /// <summary>
    /// Unsubscribes from input events to prevent duplicate event calls
    /// if this component is disabled and re-enabled
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJump;

        if (punchAction != null)
            punchAction.action.performed -= OnPunch;
    }

    /// <summary>
    /// Reads the current movement value from the assigned move input action
    /// </summary>
    private void ReadMovementInput()
    {
        if (moveAction == null)
            return;

        Move = moveAction.action.ReadValue<float>();
    }

    /// <summary>
    /// Reads whether the crouch input is currently being held
    /// </summary>
    private void ReadCrouchInput()
    {
        if (crouchAction == null)
        {
            CrouchHeld = false;
            return;
        }

        CrouchHeld = crouchAction.action.IsPressed();
    }

    /// <summary>
    /// Clears button press flags after other scripts have had a frame to read them
    /// </summary>
    private void ResetOneFrameInputs()
    {
        JumpPressed = false;
        PunchPressed = false;
    }

    /// <summary>
    /// Called when the jump input action is performed
    /// </summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    /// <summary>
    /// Called when the punch input action is performed
    /// </summary>
    private void OnPunch(InputAction.CallbackContext context)
    {
        PunchPressed = true;
    }
}