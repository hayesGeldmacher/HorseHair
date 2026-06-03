using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("movement fields")]
    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;
    [SerializeField] private bool isMovingInput = false; //is player moving
    [SerializeField] private float walkSpeed;

    [Header("Gravity Fields")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRange = 0.7f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float velocity;
    [SerializeField] private float maxVelocity;
    

    private CharacterController controller; //private movement component

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = transform.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        InputUpdate();

        if (isMovingInput) { MoveUpdate(); }
        GroundedUpdate();
        GravityUpdate();
    }

    private void GroundedUpdate()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckRange, groundMask);
    }

    private  void GravityUpdate()
    {
        velocity -= gravityStrength * Time.deltaTime;
        if(velocity < -maxVelocity) { velocity = -maxVelocity; }
        Vector3 downMovement = Vector3.up * velocity;
        controller.Move(downMovement * Time.deltaTime);
    }

    private void InputUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        isMovingInput = ((Mathf.Abs(horizontal) + Mathf.Abs(vertical)) >= 0.1f) ? true : false;

    }

    private void MoveUpdate()
    {
        //get the direction of movement from player input
        Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical);
        moveDirection.Normalize(); //stop any movement vector from being greater than 1
        //move the character controller component based on current rotation
        controller.Move(moveDirection * walkSpeed * Time.deltaTime);


    }
}
