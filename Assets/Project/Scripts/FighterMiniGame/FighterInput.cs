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

    [Tooltip("Triggered when the player presses the kick input")]
    [SerializeField] private InputActionReference kickAction;

    [Tooltip("Held when the player holds the crouch input")]
    [SerializeField] private InputActionReference crouchAction;

    [Tooltip("Triggered when the player presses the grab input")]
    [SerializeField] private InputActionReference grabAction;

    [Tooltip("Troggered when the plaeyr presses the special input")]
    [SerializeField] private InputActionReference specialAction;

    public float Move { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PunchPressed { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool KickPressed { get; private set; }
    public bool GrabPressed { get; private set; }

    public bool SpecialPressed { get; private set; }

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

    private void EnableInputActions()
    {
        if (moveAction != null)
            moveAction.action.Enable();

        if (jumpAction != null)
            jumpAction.action.Enable();

        if (punchAction != null)
            punchAction.action.Enable();

        if (kickAction != null)
            kickAction.action.Enable();

        if (crouchAction != null)
            crouchAction.action.Enable();

        if (grabAction != null)
            grabAction.action.Enable();

        if(specialAction != null)
        {
            specialAction.action.Enable();
        }
    }

    private void DisableInputActions()
    {
        if (moveAction != null)
            moveAction.action.Disable();

        if (jumpAction != null)
            jumpAction.action.Disable();

        if (punchAction != null)
            punchAction.action.Disable();

        if (kickAction != null)
            kickAction.action.Disable();

        if (crouchAction != null)
            crouchAction.action.Disable();

        if (grabAction != null)
            grabAction.action.Disable();

        if (specialAction != null)
        {
            specialAction.action.Disable();
        }
    }

    private void SubscribeToInputEvents()
    {
        if (jumpAction != null)
            jumpAction.action.performed += OnJump;

        if (punchAction != null)
            punchAction.action.performed += OnPunch;

        if (kickAction != null)
            kickAction.action.performed += OnKick;

        if (grabAction != null)
            grabAction.action.performed += OnGrab;

        if (specialAction != null)
            specialAction.action.performed += OnSpecial;
    }

    private void UnsubscribeFromInputEvents()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJump;

        if (punchAction != null)
            punchAction.action.performed -= OnPunch;

        if (kickAction != null)
            kickAction.action.performed -= OnKick;

        if (grabAction != null)
            grabAction.action.performed -= OnGrab;

        if (specialAction != null)
            specialAction.action.performed -= OnSpecial;
    }

    private void ReadMovementInput()
    {
        if (moveAction == null)
            return;

        Move = moveAction.action.ReadValue<float>();
    }

    private void ReadCrouchInput()
    {
        if (crouchAction == null)
        {
            CrouchHeld = false;
            return;
        }

        CrouchHeld = crouchAction.action.IsPressed();
    }

    private void ResetOneFrameInputs()
    {
        JumpPressed = false;
        PunchPressed = false;
        KickPressed = false;
        GrabPressed = false;
        SpecialPressed = false;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    private void OnPunch(InputAction.CallbackContext context)
    {
        PunchPressed = true;
    }

    private void OnKick(InputAction.CallbackContext context)
    {
        KickPressed = true;
    }

    private void OnGrab(InputAction.CallbackContext context)
    {
        GrabPressed = true;
    }

    private void OnSpecial(InputAction.CallbackContext context)
    {
        SpecialPressed = true;
    }
}