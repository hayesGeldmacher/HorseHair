using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField, Tooltip("How far can the camera move up and down / How fast the camera moves up and down in 360")] 
    private float _pitchClamp = 20f;
    [SerializeField, Tooltip("How far can the camera move left and right / How fast the camera moves left and right in 360")] 
    private float _yawClamp = 20f;
    [SerializeField, Tooltip("How fast the camera follows the cursor horizontally")]
    private float _followSpeedX = 2f;
    [SerializeField, Tooltip("How fast the camera follows the cursor vertically")]
    private float _followSpeedY = 2f;
    [SerializeField]
    private bool full360Camera_x = false;
    [SerializeField]
    private bool full360Camera_y = false;

    [Header("Tester Settings")]
    [SerializeField]
    public PhysicsRaycaster rayCaster;

    private Vector2 _screenCenter;
    public Vector2 _mouseInput = Vector2.zero;
    public Vector2 _currentInput = Vector2.zero;
    private CinemachinePanTilt _panTilt;
    private bool isFrozen = false;
    private Vector2 frozenPosition;

    [SerializeField] private VirtualMouseInput VM;
    [SerializeField] private PlayerInput playerInput;
    private const string gamepadScheme = "Gamepad";
    private const string mouseScheme = "Keyboard&Mouse";

    private bool UseMouse = false;
    private string previousControlScheme;
    private Coroutine enableDeviceCoroutine;

    [SerializeField] private float EnableDeviceDelay = 0.3f;
    private bool mouseEnabled = true;

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void Start()
    {
        _screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Cursor.lockState = CursorLockMode.Confined;
        Mouse.current.WarpCursorPosition(_screenCenter);
        _panTilt = GetComponent<CinemachinePanTilt>();
        Switch();
    }

    private void LateUpdate()
    {
        _currentInput.x = Mathf.Lerp(_currentInput.x, _mouseInput.x,
            Time.deltaTime * _followSpeedX);
        _currentInput.y = Mathf.Lerp(_currentInput.y, _mouseInput.y,
            Time.deltaTime * _followSpeedY);

        if (full360Camera_x)
        {
            _panTilt.PanAxis.Value += _currentInput.x * _yawClamp;
        }
        else
        {
            _panTilt.PanAxis.Value = _currentInput.x * _yawClamp;
        }

        if (full360Camera_y)
        {
            _panTilt.TiltAxis.Value += -_currentInput.y * _pitchClamp;
        }
        else
        {
            _panTilt.TiltAxis.Value = -_currentInput.y * _pitchClamp;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 offset = new Vector2(
            (mousePos.x - _screenCenter.x) / _screenCenter.x,
            (mousePos.y - _screenCenter.y) / _screenCenter.y
        );
        _mouseInput = Vector2.ClampMagnitude(offset, 1f);
    }

    public void ChangeCameraSettings(float newPitchLimit = 20, float newYawLimit = 20, float newFollowSpeedX = 2, 
        float newFollowSpeedY = 2, bool x_spin_360 = false, bool y_spin_360 = false)
    {
        _pitchClamp = newPitchLimit;
        _yawClamp = newYawLimit;
        _followSpeedX = newFollowSpeedX;
        _followSpeedY = newFollowSpeedY;
        full360Camera_x = x_spin_360;
        full360Camera_y = y_spin_360;
    }

    public void SwitchControls(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Switch();
    }

    private void Switch()
    {
        if (!UseMouse)
        {
            if (VM.m_SystemMouse != null)
                InputSystem.EnableDevice(VM.m_SystemMouse);
            VM.UseGamePad = false;
            VM.m_SystemMouse.MakeCurrent();
            UseMouse = true;
        }
        else
        {
            if (VM.m_SystemMouse != null)
                InputSystem.DisableDevice(VM.m_SystemMouse);
            VM.UseGamePad = true;
            VM.virtualMouse.MakeCurrent();
            UseMouse = false;
        }      
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        var action = obj as InputAction;
        var device = action?.activeControl?.device;
        if (device == null) return;

        if (device == VM.virtualMouse) return;

        bool isGamepadInput = device is Gamepad;
        bool isMouseOrKeyboardInput = device is Mouse;

        if (isGamepadInput)
        {
            mouseEnabled = false;
            EnableDevice();
        }
        if (isMouseOrKeyboardInput)
        {
            mouseEnabled = true;
            StopAllCoroutines();
        }

        if (isGamepadInput && UseMouse)
        {
            if (VM.m_SystemMouse != null)
                InputSystem.DisableDevice(VM.m_SystemMouse);
            VM.UseGamePad = true;
            VM.virtualMouse.MakeCurrent();
            UseMouse = false;
        }
        else if (isMouseOrKeyboardInput && !UseMouse)
        {
            if (device is Mouse mouse && mouse.delta.ReadValue().sqrMagnitude < 0.01f)
                return;

            if (VM.m_SystemMouse != null)
                InputSystem.EnableDevice(VM.m_SystemMouse);
            VM.UseGamePad = false;
            VM.m_SystemMouse.MakeCurrent();
            UseMouse = true;
        }
    }

    public void EnableDevice()
    {
        if (enableDeviceCoroutine != null)
        {
            StopCoroutine(enableDeviceCoroutine);
        }
        enableDeviceCoroutine = StartCoroutine(EnableDeviceAfterDelay());
    }

    private IEnumerator EnableDeviceAfterDelay()
    { 
        yield return new WaitForSeconds(EnableDeviceDelay);
        if (mouseEnabled != true)
        {
            enableDeviceCoroutine = null;
            mouseEnabled = true;
            StopAllCoroutines();

            if (VM.m_SystemMouse != null)
                InputSystem.EnableDevice(VM.m_SystemMouse);
            VM.UseGamePad = false;
            VM.m_SystemMouse.MakeCurrent();
            UseMouse = true;
        }
    }
}
