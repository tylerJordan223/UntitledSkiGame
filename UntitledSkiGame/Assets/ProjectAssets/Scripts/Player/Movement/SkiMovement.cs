using Global_Input;
using UnityEngine;

public class SkiMovement : MonoBehaviour
{
    #region singleton
    public static SkiMovement Instance
    {
        get
        {
            return instance;
        }
    }

    private static SkiMovement instance = null;
    #endregion singleton

    private void Awake()
    {
        //initialize the singleton
        if (instance)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    [Header("Movement:")]
    public float speed;
    public float ground_drag;
    public float slopeAcceleration;
    
    public Transform orientation;

    [Header("Grounding:")]
    public float height;
    public LayerMask groundMask;
    public bool grounded;

    [Header("Gravity/Acceleration:")]
    public float maxGroundDistance = 0.5f;
    public float maxSlopeAngle = 45;
    public CapsuleCollider playerCollider;

    //saved values for calculation
    private float slopeAngle = 180f; //flat ground at default
    private Vector3 groundNormal; 
    private Rigidbody rb;
    
    //movement values
    private Vector3 moveDirection;
    private float horizontal_input;
    private float vertical_input;

    //basic input
    private GlobalInput input;

    private void Start()
    {
        //initialize variables
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //enable input
        input = new GlobalInput();
        input.Mounted.Enable();
    }

    private void OnDisable()
    {
        //disable input
        input.Mounted.Disable();
    }

    private void FixedUpdate()
    {
        //only check for the groundedness if you are still grounded (this gets reset on collision
        if(grounded)
        {
            CheckGrounded();
        }
        //always move/apply the gravity
        MovePlayer();
        ApplyGravity();
    }

    private void Update()
    {
        //handle the drag
        if(grounded)
        {
            rb.linearDamping = ground_drag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }

    //function used to move the player
    private void MovePlayer()
    {
        //get the player input
        Vector2 movement = input.Player.Move.ReadValue<Vector2>();

        //assign movement
        horizontal_input = movement.x;
        vertical_input = movement.y;

        //calculating the initial movement direction
        Vector3 inputDirection = orientation.forward * vertical_input + orientation.right * horizontal_input;

        //important case for actually doing this
        if(grounded && inputDirection.magnitude > 0.1f)
        {
            //project that movement onto the slope
            moveDirection = Vector3.ProjectOnPlane(inputDirection, groundNormal);

            //apply speed
            Vector3 adjusted_movement = moveDirection * speed;

            //transition to the goal velocity
            Vector3 current = rb.linearVelocity;
            Vector3 change = adjusted_movement - current;
            change.y = 0; //don't affect vertical velocity

            //apply acceleration
            rb.AddForce(change * 10f, ForceMode.Acceleration);
        }
    }

    //applying gravity
    private void ApplyGravity()
    {
        if(!grounded)
        {
            //normal gravity in air
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
        else
        {
            if(slopeAngle > 1f)
            {
                //apply force down the slope (downhill acceleration)
                Vector3 slopeGravity = Vector3.ProjectOnPlane(Vector3.down, groundNormal) * slopeAcceleration;
                rb.AddForce(slopeGravity, ForceMode.Acceleration);
            }

            //increase the stick force based on horizontal speed
            float horizontal_speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            float force = 10f + (horizontal_speed * 2f); //10 at default, more depending on speed
            rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
        }
    }

    //function to find out whether or not we are grounded + get important variables
    private void CheckGrounded()
    {
        //we are going to raycast in front of the player to be predictive//
        float rayStartHeight = playerCollider.height / 2f;

        //cast from current position + offset
        Vector3 rayStart = transform.position - new Vector3(0f, rayStartHeight - 0.3f, 0f);

        //perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, maxGroundDistance, groundMask))
        {
            //get the slope angle
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if(slopeAngle <= maxSlopeAngle)
            {
                //within range to movewith slope
                grounded = true;
                groundNormal = hit.normal;
            }
            else
            {
                //slope is too steep
                grounded = false;
                groundNormal = Vector3.up;
            }
        }
        else
        {
            //not on the ground
            grounded = false;
            groundNormal = Vector3.up;
        }
    }

    //COLLISION//
    private void OnCollisionStay(Collision collision)
    {
        //used to reset the grounded variable
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }
}
