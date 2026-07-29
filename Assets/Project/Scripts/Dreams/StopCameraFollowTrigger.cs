using System.Collections;
using UnityEngine;

/// <summary>
/// Freezes the dream camera on a trigger and makes the player walk out of the right side
/// </summary>
[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(Collider))]
public class StopCameraFollowTrigger : MonoBehaviour
{
    [Tooltip("Player speed")]
    [Min(0f)]
    [SerializeField] private float exitWalkSpeed = 5f;

    [Tooltip("Extra viewport space before movement stops.")]
    [Min(0f)]
    [SerializeField] private float exitPadding = 0.02f;

    private DSCam cameraFollowScript;
    private Camera sceneCamera;
    private FightCharacter playerCharacter;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private bool hasTriggered;
    private bool isForcingExitWalk;

    private void Awake()
    {
        cameraFollowScript = FindAnyObjectByType<DSCam>();
        sceneCamera = Camera.main;

        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        FightCharacter player = other.GetComponentInParent<FightCharacter>();

        if (player == null ||
            !player.CompareTag("Player") ||
            !player.IsDreamTraversalMode)
            return;

        hasTriggered = true;

        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;
        else
            Debug.LogWarning("StopCameraFollowTrigger could not find DSCam.", this);

        playerCharacter = player;
        playerRigidbody = player.GetComponent<Rigidbody>();
        playerAnimator = player.GetComponentInChildren<Animator>();

        if (playerRigidbody == null)
        {
            Debug.LogError(
                "StopCameraFollowTrigger could not find the player's Rigidbody.",
                player
            );
            return;
        }
        playerCharacter.enabled = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("walkNormal", true);
            playerAnimator.SetBool("shuffling", true);
        }

        isForcingExitWalk = true;
        StartCoroutine(StopPlayerAfterLeavingFrame(player));
    }

    private void FixedUpdate()
    {
        if (!isForcingExitWalk || playerRigidbody == null)
            return;

        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.x = exitWalkSpeed;
        playerRigidbody.linearVelocity = velocity;
    }

    private void LateUpdate()
    {
        if (!isForcingExitWalk || playerAnimator == null)
            return;

        playerAnimator.SetBool("walkNormal", true);
        playerAnimator.SetBool("shuffling", true);
    }

    private IEnumerator StopPlayerAfterLeavingFrame(FightCharacter player)
    {
        Renderer[] playerRenderers =
            player.GetComponentsInChildren<Renderer>(true);

        // Let the player move for at least one rendered frame before checking.
        yield return null;

        while (player != null &&
               !IsFullyPastRightEdge(player.transform, playerRenderers))
        {
            yield return null;
        }

        StopExitWalk();
    }

    private void StopExitWalk()
    {
        isForcingExitWalk = false;

        if (playerRigidbody != null)
        {
            Vector3 velocity = playerRigidbody.linearVelocity;
            velocity.x = 0f;
            playerRigidbody.linearVelocity = velocity;
        }

        if (playerAnimator != null)
            playerAnimator.SetBool("shuffling", false);
    }

    private bool IsFullyPastRightEdge(
        Transform playerTransform,
        Renderer[] playerRenderers)
    {
        if (sceneCamera == null)
            sceneCamera = Camera.main;

        if (sceneCamera == null)
            return false;

        bool foundEnabledRenderer = false;

        foreach (Renderer playerRenderer in playerRenderers)
        {
            if (playerRenderer == null || !playerRenderer.enabled)
                continue;

            foundEnabledRenderer = true;

            Bounds bounds = playerRenderer.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(
                            extents,
                            new Vector3(x, y, z)
                        );

                        Vector3 viewportPoint =
                            sceneCamera.WorldToViewportPoint(corner);

                        if (viewportPoint.z > 0f &&
                            viewportPoint.x <= 1f + exitPadding)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        if (foundEnabledRenderer)
            return true;

        Vector3 playerViewportPoint =
            sceneCamera.WorldToViewportPoint(playerTransform.position);

        return playerViewportPoint.z > 0f &&
               playerViewportPoint.x > 1f + exitPadding;
    }
}
