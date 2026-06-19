using UnityEngine;

public class DSCam : MonoBehaviour
{
    public Transform player;
    public Transform leftBound;

    public float playerLeftOffset = 5f;
    public float smoothSpeed = 5f;
    public float leftBoundOffset = 8f;

    private float furthestCameraX;

    void Start()
    {
        furthestCameraX = transform.position.x;
    }

    void LateUpdate()
    {
        if (player == null) return;

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
}