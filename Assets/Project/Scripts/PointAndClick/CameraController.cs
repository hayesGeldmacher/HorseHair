using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField, Tooltip("How far can the camera move up and down")] 
    private float _pitchClamp = 20f;
    [SerializeField, Tooltip("How far can the camera move left and right")] 
    private float _yawClamp = 20f;
    [SerializeField, Tooltip("How fast the camera follows the cursor horizontally")]
    private float _followSpeedX = 2f;
    [SerializeField, Tooltip("How fast the camera follows the cursor vertically")]
    private float _followSpeedY = 2f;

    private Vector2 _screenCenter;
    public Vector2 _mouseInput = Vector2.zero;
    public Vector2 _currentInput = Vector2.zero;
    private CinemachinePanTilt _panTilt;

    private void Start()
    {
        _screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Cursor.lockState = CursorLockMode.Confined;
        Mouse.current.WarpCursorPosition(_screenCenter);
        _panTilt = GetComponent<CinemachinePanTilt>();
    }

    private void LateUpdate()
    {
        _currentInput.x = Mathf.Lerp(_currentInput.x, _mouseInput.x, 
            Time.deltaTime * _followSpeedX);
        _currentInput.y = Mathf.Lerp(_currentInput.y, _mouseInput.y, 
            Time.deltaTime * _followSpeedY);


        _panTilt.PanAxis.Value = _currentInput.x * _yawClamp;
        _panTilt.TiltAxis.Value = -_currentInput.y * _pitchClamp;
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
        float newFollowSpeedY = 2)
    {
        _pitchClamp = newPitchLimit;
        _yawClamp = newYawLimit;
        _followSpeedX = newFollowSpeedX;
        _followSpeedY = newFollowSpeedY;
    }
}
