using UnityEngine;

/// <summary>
///  Camera controller that keeps both fighters visible while allowing them to move freely within a comfortable screen area
/// The camera only moves when fighters approach the edges of this area, providing a smooth and dynamic view of the action without constant camera movement
/// It also includes optional bounds to prevent showing past the visible stage edges
/// </summary>
public class FightCameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Player transform")]
    [SerializeField] private Transform player;

    [Tooltip("Enemy transform")]
    [SerializeField] private Transform enemy;

    [Header("Screen Zone")]
    [Tooltip("How much horizontal screen space the fighters can use before the camera moves")]
    [Range(0.1f, 0.95f)]
    [SerializeField] private float horizontalDeadZone = 0.65f;

    [Tooltip("Extra world-space padding between fighters and the camera edge")]
    [SerializeField] private float edgePadding = 0.75f;

    [Header("Follow")]
    [Tooltip("How smoothly the camera follows when fighters reach the edge zone")]
    [SerializeField] private float followSmoothTime = 0.3f;

    [Tooltip("Maximum camera movement speed")]
    [SerializeField] private float maxFollowSpeed = 8f;

    [Header("Camera Bounds")]
    [Tooltip("If true, camera cannot move past the visible stage edges")]
    [SerializeField] private bool useCameraBounds = true;

    [Tooltip("Left visible edge of the stage")]
    [SerializeField] private float visibleStageMinX = -11f;

    [Tooltip("Right visible edge of the stage")]
    [SerializeField] private float visibleStageMaxX = 11f;

    private Camera fightCamera;
    private Vector3 startPosition;
    private Vector3 followVelocity;

    private void Reset()
    {
        fightCamera = GetComponent<Camera>();
    }

    private void Awake()
    {
        if (fightCamera == null)
            fightCamera = GetComponent<Camera>();

        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null || enemy == null || fightCamera == null)
            return;

        FollowOnlyWhenNeeded();
    }

    /// <summary>
    /// Moves the camera only when fighters are close to leaving the comfortable screen area
    /// </summary>
    private void FollowOnlyWhenNeeded()
    {
        float halfCameraWidth = GetHalfCameraWidth();
        float currentCameraX = transform.position.x;
        float targetCameraX = GetTargetCameraX(currentCameraX, halfCameraWidth);

        if (useCameraBounds)
            targetCameraX = ClampCameraX(targetCameraX, halfCameraWidth);

        Vector3 targetPosition = new Vector3(
            targetCameraX,
            startPosition.y,
            startPosition.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref followVelocity,
            followSmoothTime,
            maxFollowSpeed
        );
    }

    private float GetTargetCameraX(float currentCameraX, float halfCameraWidth)
    {
        float allowedHalfWidth = halfCameraWidth * horizontalDeadZone;

        float leftAllowedX = currentCameraX - allowedHalfWidth + edgePadding;
        float rightAllowedX = currentCameraX + allowedHalfWidth - edgePadding;

        float leftFighterX = Mathf.Min(player.position.x, enemy.position.x);
        float rightFighterX = Mathf.Max(player.position.x, enemy.position.x);

        if (leftFighterX < leftAllowedX)
            return currentCameraX - (leftAllowedX - leftFighterX);

        if (rightFighterX > rightAllowedX)
            return currentCameraX + (rightFighterX - rightAllowedX);

        return currentCameraX;
    }

    /// <summary>
    /// Returns half the camera width in world units
    /// </summary>
    private float GetHalfCameraWidth()
    {
        if (fightCamera.orthographic)
            return fightCamera.orthographicSize * fightCamera.aspect;

        return Mathf.Abs(transform.position.z - startPosition.z);
    }

    /// <summary>
    /// Prevents the camera from showing past the visible stage edges
    /// </summary>
    private float ClampCameraX(float targetX, float halfCameraWidth)
    {
        float minCameraX = visibleStageMinX + halfCameraWidth;
        float maxCameraX = visibleStageMaxX - halfCameraWidth;

        if (minCameraX > maxCameraX)
            return (visibleStageMinX + visibleStageMaxX) * 0.5f;

        return Mathf.Clamp(targetX, minCameraX, maxCameraX);
    }
}