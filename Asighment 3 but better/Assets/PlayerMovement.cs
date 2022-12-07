using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Movement")]
    private float moveSpeed;
    public float walkspeed;

    public float sprintspeed;

    public float groundDrag;


    private float jumpForce = 5;
    public float jumpCooldown;
    public float airMultiplier;
    bool readytojump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;



    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;


    [Header("Ground Check")]
    public float playerHeight;
    public float groundDistance = 0.4f;
    public Transform groundCheck;

    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;


    //states
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching

    }






    void Start()
    {
        //sets everything up
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readytojump = true;
        startYScale = transform.localScale.y;
    }

   void minusminus()
    {
        moveSpeed--;
    }

    void StateHandler()
    {


        if (Input.GetKey(crouchKey))
        {

            state = MovementState.crouching;
            moveSpeed = crouchSpeed;

        }
    

        
       


    

        //running
        if (grounded && Input.GetKey(sprintKey) && state != MovementState.crouching)
        {
            if (transform.localScale == new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
            state = MovementState.sprinting;
            moveSpeed = sprintspeed;
            jumpForce = 7;
        }
        // walking
        else if(grounded)
        {
            if (state == MovementState.crouching)
            {
                moveSpeed = crouchSpeed;
                jumpForce = 6;
            }
            else if (state != MovementState.crouching)
            {
                state = MovementState.walking;
                moveSpeed = walkspeed;
                jumpForce = 5;
            }
            else
            {
                moveSpeed = walkspeed;
                jumpForce = 5;
            }

        }
        // mode is air
        else
        {
            state = MovementState.air;
            

        }
        
    }


    void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        if (Input.GetKey(jumpKey) && readytojump && grounded)
        {
            readytojump = false;

            Jump();

            Invoke("ResetJump", jumpCooldown);

        }
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else if (!grounded)
        {
            rb.drag = 0;

        }

        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (grounded)
            {
                moveSpeed = crouchSpeed;

                //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
         
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            state = MovementState.walking;
            if (state == MovementState.sprinting)
            {
                moveSpeed = sprintspeed;

            }
            else if (state == MovementState.walking)
            {
                moveSpeed = walkspeed;

            }

        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }


    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y >0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);

        }
    }
    void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatvel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatvel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatvel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    void ResetJump()
    {
        exitingSlope = false;

        readytojump = true;
    }
  
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;




        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
