using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Look Fields")]
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    private float rotationX, rotationZ = 0;
    private float rotationForward;
    private float moveX, moveY;

    [Header("Assign Fields")]
    [SerializeField] private Transform playerBody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("ControllerX") * sensitivityX;
        float mouseY = Input.GetAxis("ControllerY") * sensitivityY;
        //don't calculate rotation is player isn't moving the camera!
        if(Mathf.Abs(mouseX) + Mathf.Abs(mouseY) <= 0) { return; }

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        //rotate the player's body side to side while aiming with camera
        playerBody.Rotate(Vector3.up * mouseX);

        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
}
