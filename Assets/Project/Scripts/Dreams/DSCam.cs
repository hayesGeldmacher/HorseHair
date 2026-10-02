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

    void Start()
    {
        furthestCameraX = transform.position.x;
    }

    void LateUpdate()
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
