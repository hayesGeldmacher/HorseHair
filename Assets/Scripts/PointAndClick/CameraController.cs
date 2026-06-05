using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float _pitchSensitivity = 10f;
    [SerializeField, Tooltip("How far can the camera move up and down")] private float _pitchClamp = 20f;
    [SerializeField] private float _yawSensitivity = 10f;
    [SerializeField, Tooltip("How far can the camera move left and right")] private float _yawClamp = 20f;
    [SerializeField] bool _UseSensitivity = false;

    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

    private Vector2 _mouseInput = new Vector2(0, 0);
    private float _pitch = 0f;
    private float _yaw = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void LateUpdate()
    {
        if (_UseSensitivity)
        {
            _pitch -= _mouseInput.y * _pitchSensitivity * Time.fixedDeltaTime;
            _pitch = Mathf.Clamp(_pitch, -_pitchClamp, _pitchClamp);
            _yaw += _mouseInput.x * _yawSensitivity * Time.fixedDeltaTime;
            _yaw = Mathf.Clamp(_yaw, -_yawClamp, _yawClamp);
            transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
        }
        else
        {
            _yaw = _mouseInput.x * _yawClamp;
            _pitch = -_mouseInput.y * _pitchClamp;
            transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (_UseSensitivity)
        {
            _mouseInput = context.ReadValue<Vector2>();
        }
        else
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 offset = new Vector2(
                    (mousePos.x - screenCenter.x) / screenCenter.x,
                    (mousePos.y - screenCenter.y) / screenCenter.y
            );
            _mouseInput = Vector2.ClampMagnitude(offset, 1f);
        }
    }
}
