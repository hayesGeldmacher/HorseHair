using UnityEngine;

/// <summary>
/// Camera for fighting game that supports dynamic zooming, camera bounds, and camera shake effects.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FightCameraFollow : MonoBehaviour
{
    #region Inspector References

    [Header("Targets")]
    [Tooltip("Player transform")]
    [SerializeField] private Transform player;

    [Tooltip("Enemy transform")]
    [SerializeField] private Transform enemy;

    #endregion

    #region Framing

    [Header("Screen Framing")]
    [Tooltip("Percentage of horizontal screen space the fighters may occupy")]
    [Range(0.1f, 0.95f)]
    [SerializeField] private float horizontalScreenZone = 0.78f;

    [Tooltip("Percentage of vertical screen space the fighters may occupy")]
    [Range(0.1f, 0.95f)]
    [SerializeField] private float verticalScreenZone = 0.72f;

    [Tooltip("Extra horizontal space around the fighters")]
    [SerializeField] private float horizontalPadding = 0.8f;

    [Tooltip("Extra vertical space around the fighters")]
    [SerializeField] private float verticalPadding = 1.2f;

    [Tooltip("Allows the camera to follow fighters vertically")]
    [SerializeField] private bool followVertically = true;

    [Tooltip("Places the camera this far above the fighters")]
    [SerializeField] private float verticalOffset = 1.2f;

    #endregion

    #region Follow Settings

    [Header("Follow")]
    [Tooltip("How quickly the camera follows the fighters")]
    [SerializeField] private float followSmoothTime = 0.18f;

    [Tooltip("Maximum camera movement speed")]
    [SerializeField] private float maxFollowSpeed = 14f;

    #endregion

    #region Zoom Settings

    [Header("Dynamic Zoom")]
    [Tooltip("Automatically zooms to keep both fighters visible")]
    [SerializeField] private bool useDynamicZoom = true;

    [Tooltip("Closest orthographic zoom")]
    [SerializeField] private float minOrthographicSize = 4.5f;

    [Tooltip("Farthest orthographic zoom")]
    [SerializeField] private float maxOrthographicSize = 7f;

    [Tooltip("Closest perspective field of view")]
    [SerializeField] private float minFieldOfView = 35f;

    [Tooltip("Farthest perspective field of view")]
    [SerializeField] private float maxFieldOfView = 60f;

    [Tooltip("How quickly the camera changes zoom")]
    [SerializeField] private float zoomSmoothTime = 0.25f;

    #endregion

    #region Camera Bounds

    [Header("Camera Bounds")]
    [Tooltip("Prevents the camera from showing past the horizontal stage edges")]
    [SerializeField] private bool useHorizontalBounds = true;

    [Tooltip("Left visible edge of the stage")]
    [SerializeField] private float visibleStageMinX = -11f;

    [Tooltip("Right visible edge of the stage")]
    [SerializeField] private float visibleStageMaxX = 11f;

    [Tooltip("Prevents the camera from showing past vertical stage edges")]
    [SerializeField] private bool useVerticalBounds;

    [Tooltip("Lowest visible point of the stage")]
    [SerializeField] private float visibleStageMinY = -2f;

    [Tooltip("Highest visible point of the stage")]
    [SerializeField] private float visibleStageMaxY = 10f;

    #endregion

    #region Camera Shake

    [Header("Camera Shake")]
    [Tooltip("Default duration used by PlayShake")]
    [SerializeField] private float defaultShakeDuration = 0.12f;

    [Tooltip("Default strength used by PlayShake")]
    [SerializeField] private float defaultShakeStrength = 0.12f;

    #endregion

    #region Runtime State

    private Camera fightCamera;

    private Vector3 startingPosition;
    private Vector3 smoothedPosition;
    private Vector3 followVelocity;

    private float zoomVelocity;

    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;

    #endregion

    #region Unity Lifecycle

    private void Reset()
    {
        fightCamera = GetComponent<Camera>();
    }

    private void Awake()
    {
        fightCamera = GetComponent<Camera>();

        startingPosition = transform.position;
        smoothedPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null || enemy == null || fightCamera == null)
            return;

        UpdateDynamicZoom();

        float halfCameraHeight = GetHalfCameraHeight();
        float halfCameraWidth = halfCameraHeight * fightCamera.aspect;

        Vector3 targetPosition = CalculateTargetPosition(
            halfCameraWidth,
            halfCameraHeight
        );

        targetPosition = ApplyCameraBounds(
            targetPosition,
            halfCameraWidth,
            halfCameraHeight
        );

        smoothedPosition = Vector3.SmoothDamp(
            smoothedPosition,
            targetPosition,
            ref followVelocity,
            followSmoothTime,
            maxFollowSpeed
        );

        transform.position = smoothedPosition + CalculateShakeOffset();
    }

    private void OnValidate()
    {
        minOrthographicSize = Mathf.Max(0.01f, minOrthographicSize);
        maxOrthographicSize = Mathf.Max(
            minOrthographicSize,
            maxOrthographicSize
        );

        minFieldOfView = Mathf.Clamp(minFieldOfView, 1f, 179f);
        maxFieldOfView = Mathf.Clamp(
            maxFieldOfView,
            minFieldOfView,
            179f
        );

        followSmoothTime = Mathf.Max(0.01f, followSmoothTime);
        zoomSmoothTime = Mathf.Max(0.01f, zoomSmoothTime);
        maxFollowSpeed = Mathf.Max(0.01f, maxFollowSpeed);

        horizontalPadding = Mathf.Max(0f, horizontalPadding);
        verticalPadding = Mathf.Max(0f, verticalPadding);
    }

    #endregion

    #region Camera Positioning

    private Vector3 CalculateTargetPosition(
        float halfCameraWidth,
        float halfCameraHeight)
    {
        float leftFighterX = Mathf.Min(
            player.position.x,
            enemy.position.x
        );

        float rightFighterX = Mathf.Max(
            player.position.x,
            enemy.position.x
        );

        float usableHalfWidth =
            halfCameraWidth * horizontalScreenZone;

        float targetX = CalculateAxisTarget(
            smoothedPosition.x,
            leftFighterX,
            rightFighterX,
            usableHalfWidth,
            horizontalPadding
        );

        float targetY = startingPosition.y;

        if (followVertically)
        {
            float lowestFighterY = Mathf.Min(
                player.position.y,
                enemy.position.y
            ) + verticalOffset;

            float highestFighterY = Mathf.Max(
                player.position.y,
                enemy.position.y
            ) + verticalOffset;

            float usableHalfHeight =
                halfCameraHeight * verticalScreenZone;

            targetY = CalculateAxisTarget(
                smoothedPosition.y,
                lowestFighterY,
                highestFighterY,
                usableHalfHeight,
                verticalPadding
            );
        }

        return new Vector3(
            targetX,
            targetY,
            startingPosition.z
        );
    }

    /// <summary>
    /// Returns the nearest camera center that keeps both target values
    /// inside the usable screen area.
    /// </summary>
    private float CalculateAxisTarget(
        float currentCenter,
        float minimumTarget,
        float maximumTarget,
        float usableHalfExtent,
        float padding)
    {
        if (usableHalfExtent <= padding)
            return (minimumTarget + maximumTarget) * 0.5f;

        float minimumCameraCenter =
            maximumTarget - usableHalfExtent + padding;

        float maximumCameraCenter =
            minimumTarget + usableHalfExtent - padding;

        // Both fighters cannot fit at the current zoom.
        // Center between them while the zoom system catches up.
        if (minimumCameraCenter > maximumCameraCenter)
            return (minimumTarget + maximumTarget) * 0.5f;

        return Mathf.Clamp(
            currentCenter,
            minimumCameraCenter,
            maximumCameraCenter
        );
    }

    #endregion

    #region Dynamic Zoom

    private void UpdateDynamicZoom()
    {
        if (!useDynamicZoom)
            return;

        float horizontalSeparation =
            Mathf.Abs(player.position.x - enemy.position.x);

        float verticalSeparation =
            Mathf.Abs(player.position.y - enemy.position.y);

        float requiredHorizontalHalfExtent =
            horizontalSeparation * 0.5f + horizontalPadding;

        float requiredVerticalHalfExtent =
            verticalSeparation * 0.5f + verticalPadding;

        float requiredHeightFromHorizontal =
            requiredHorizontalHalfExtent /
            Mathf.Max(
                fightCamera.aspect * horizontalScreenZone,
                0.01f
            );

        float requiredHeightFromVertical =
            requiredVerticalHalfExtent /
            Mathf.Max(verticalScreenZone, 0.01f);

        float requiredHalfHeight = Mathf.Max(
            requiredHeightFromHorizontal,
            requiredHeightFromVertical
        );

        if (fightCamera.orthographic)
        {
            UpdateOrthographicZoom(requiredHalfHeight);
        }
        else
        {
            UpdatePerspectiveZoom(requiredHalfHeight);
        }
    }

    private void UpdateOrthographicZoom(float requiredHalfHeight)
    {
        float targetSize = Mathf.Clamp(
            requiredHalfHeight,
            minOrthographicSize,
            maxOrthographicSize
        );

        // Prevents zooming wider than the stage itself.
        if (useHorizontalBounds)
        {
            float stageWidth =
                visibleStageMaxX - visibleStageMinX;

            float maximumSizeFromStage =
                stageWidth * 0.5f /
                Mathf.Max(fightCamera.aspect, 0.01f);

            targetSize = Mathf.Min(
                targetSize,
                maximumSizeFromStage
            );
        }

        fightCamera.orthographicSize = Mathf.SmoothDamp(
            fightCamera.orthographicSize,
            targetSize,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private void UpdatePerspectiveZoom(float requiredHalfHeight)
    {
        float distanceToFighterPlane =
            GetDistanceToFighterPlane();

        float requiredFieldOfView =
            2f *
            Mathf.Atan(
                requiredHalfHeight /
                Mathf.Max(distanceToFighterPlane, 0.01f)
            ) *
            Mathf.Rad2Deg;

        float targetFieldOfView = Mathf.Clamp(
            requiredFieldOfView,
            minFieldOfView,
            maxFieldOfView
        );

        fightCamera.fieldOfView = Mathf.SmoothDamp(
            fightCamera.fieldOfView,
            targetFieldOfView,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    #endregion

    #region Camera Size

    private float GetHalfCameraHeight()
    {
        if (fightCamera.orthographic)
            return fightCamera.orthographicSize;

        float distanceToFighterPlane =
            GetDistanceToFighterPlane();

        return Mathf.Tan(
            fightCamera.fieldOfView *
            0.5f *
            Mathf.Deg2Rad
        ) * distanceToFighterPlane;
    }

    private float GetDistanceToFighterPlane()
    {
        Vector3 fighterCenter =
            (player.position + enemy.position) * 0.5f;

        float distance = Mathf.Abs(
            Vector3.Dot(
                fighterCenter - smoothedPosition,
                transform.forward
            )
        );

        return Mathf.Max(distance, 0.01f);
    }

    #endregion

    #region Bounds

    private Vector3 ApplyCameraBounds(
        Vector3 targetPosition,
        float halfCameraWidth,
        float halfCameraHeight)
    {
        if (useHorizontalBounds)
        {
            float minimumCameraX =
                visibleStageMinX + halfCameraWidth;

            float maximumCameraX =
                visibleStageMaxX - halfCameraWidth;

            if (minimumCameraX > maximumCameraX)
            {
                targetPosition.x =
                    (visibleStageMinX + visibleStageMaxX) * 0.5f;
            }
            else
            {
                targetPosition.x = Mathf.Clamp(
                    targetPosition.x,
                    minimumCameraX,
                    maximumCameraX
                );
            }
        }

        if (useVerticalBounds)
        {
            float minimumCameraY =
                visibleStageMinY + halfCameraHeight;

            float maximumCameraY =
                visibleStageMaxY - halfCameraHeight;

            if (minimumCameraY > maximumCameraY)
            {
                targetPosition.y =
                    (visibleStageMinY + visibleStageMaxY) * 0.5f;
            }
            else
            {
                targetPosition.y = Mathf.Clamp(
                    targetPosition.y,
                    minimumCameraY,
                    maximumCameraY
                );
            }
        }

        return targetPosition;
    }

    #endregion

    #region Camera Shake

    /// <summary>
    /// Plays a shake using the default Inspector values.
    /// Can be called by an animation event or another script.
    /// </summary>
    public void PlayShake()
    {
        Shake(defaultShakeDuration, defaultShakeStrength);
    }

    /// <summary>
    /// Starts a custom camera shake.
    /// </summary>
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

        shakeTimer -= Time.deltaTime;

        float remainingPercentage =
            Mathf.Clamp01(shakeTimer / shakeDuration);

        Vector2 randomOffset =
            Random.insideUnitCircle *
            shakeStrength *
            remainingPercentage;

        if (shakeTimer <= 0f)
            shakeStrength = 0f;

        return new Vector3(
            randomOffset.x,
            randomOffset.y,
            0f
        );
    }

    #endregion
}