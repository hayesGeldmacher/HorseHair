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
    [SerializeField, Tooltip("How fast the camera follows the cursor")] 
    private float _followSpeed = 5f;

    private Vector2 _screenCenter;
    private Vector2 _mouseInput = Vector2.zero;
    private Vector2 _currentInput = Vector2.zero;
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
        _currentInput = Vector2.Lerp(_currentInput, _mouseInput, 
            Time.deltaTime * _followSpeed);

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

    public void ChangeCameraSettings(float newPitchLimit = 20, float newYawLimit = 20, float newFollowSpeed = 5)
    {
        _pitchClamp = newPitchLimit;
        _yawClamp = newYawLimit;
        _followSpeed = newFollowSpeed;
    }
}
