using System.Runtime.CompilerServices;
using UnityEngine;

public class DSCam : MonoBehaviour
{
    public Transform player;
    public Transform leftBound;

    [SerializeField] private FightCharacter fightCharacter;
    [SerializeField] private Camera cam;
    [SerializeField] private float goalZoom;
    [SerializeField] private float zoomSpeed;
    private float startZoom;
    private bool isZooming = false;
    public float playerLeftOffset = 5f;
    public float smoothSpeed = 5f;
    public float leftBoundOffset = 8f;
    float t = 0;

    private bool followingPlayer = true;
    private bool finalFollowPlayer = true;

    private float furthestCameraX;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;
    private Vector3 shakeOffset;

    void Start()
    {
        furthestCameraX = transform.position.x;
    }

    void LateUpdate()
    {
        transform.position -= shakeOffset;
        UpdateCameraFollow();
        shakeOffset = CalculateShakeOffset();
        transform.position += shakeOffset;
    }

    private void OnDisable()
    {
        transform.position -= shakeOffset;
        shakeOffset = Vector3.zero;
        shakeTimer = 0f;
    }

    public void Shake(float duration, float strength)
    {
        shakeDuration = Mathf.Max(duration, 0.01f);
        shakeTimer = shakeDuration;
        shakeStrength = Mathf.Max(strength, 0f);
    }

    private Vector3 CalculateShakeOffset()
    {
        if (shakeTimer <= 0f)
            return Vector3.zero;

        shakeTimer = Mathf.Max(0f, shakeTimer - Time.unscaledDeltaTime);
        float remainingPercentage = Mathf.Clamp01(shakeTimer / shakeDuration);
        Vector2 offset = Random.insideUnitCircle * shakeStrength * remainingPercentage;
        return new Vector3(offset.x, offset.y, 0f);
    }

    private void UpdateCameraFollow()
    {

        if (isZooming)
        {
            if(t < 1.0)
            {
                t += Time.deltaTime * zoomSpeed;
                if(t > 1.0) { t = 1.0f; isZooming = false; }
                float newFOV = Mathf.Lerp(startZoom, goalZoom, t);
                cam.fieldOfView = newFOV;
            }
        }




        if (player == null || !finalFollowPlayer) return;

        if(!followingPlayer)
        {
            if (smoothSpeed > 0 && fightCharacter.movingForward)
            {
                smoothSpeed -= Time.deltaTime * 3;
                if(smoothSpeed < 0)
                {
                    smoothSpeed = 0;
                    finalFollowPlayer = false;
                }
            }
        }

        float desiredCameraX = player.position.x + playerLeftOffset;

        // camera only moves right
        furthestCameraX = Mathf.Max(furthestCameraX, desiredCameraX);

        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(furthestCameraX, transform.position.y, transform.position.z),
            smoothSpeed * Time.deltaTime
        );

        // move left boundary behind camera/player
        if (leftBound != null)
        {
            leftBound.position = new Vector3(
                transform.position.x - leftBoundOffset,
                leftBound.position.y,
                leftBound.position.z
            );
        }
    }

    public void DisableCameraFollow()
    {
        followingPlayer = false;
    }

    public void StartZooming()
    {
        isZooming = true;
        startZoom = cam.fieldOfView;
    }
}
