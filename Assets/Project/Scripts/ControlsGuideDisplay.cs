using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsGuideDisplay : MonoBehaviour
{
    [SerializeField] private GameObject mouseKeyboardGuide;
    [SerializeField] private GameObject controllerGuide;

    private static bool usingController;
    public static bool UsingController => usingController;

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        RefreshGuide();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(
        object changedObject,
        InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed)
            return;

        InputAction action = changedObject as InputAction;
        InputControl control = action?.activeControl;

        if (control == null)
            return;

        if (control.device is Gamepad)
        {
            if (control.EvaluateMagnitude() < 0.25f)
                return;

            SetInputType(true);
        }
        else if (control.device is Keyboard keyboard)
        {
            if (keyboard.anyKey.isPressed)
                SetInputType(false);
        }
        else if (control.device is Mouse mouse)
        {
            if (!mouse.native)
                return;

            bool moved =
                mouse.delta.ReadValue().sqrMagnitude > 0.25f;

            bool clicked =
                mouse.leftButton.isPressed ||
                mouse.rightButton.isPressed ||
                mouse.middleButton.isPressed;

            bool scrolled =
                mouse.scroll.ReadValue().sqrMagnitude > 0f;

            if (moved || clicked || scrolled)
                SetInputType(false);
        }
    }

    private void SetInputType(bool controller)
    {
        if (usingController == controller)
            return;

        usingController = controller;
        RefreshGuide();
    }

    private void RefreshGuide()
    {
        if (mouseKeyboardGuide != null)
            mouseKeyboardGuide.SetActive(!usingController);

        if (controllerGuide != null)
            controllerGuide.SetActive(usingController);
    }
}