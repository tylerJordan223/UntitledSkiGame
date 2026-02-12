using Global_Input;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region singleton
    public static PlayerMovement Instance
    {
        get
        {
            return instance;
        }
    }

    private static PlayerMovement instance = null;
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

    [Header("Grounding:")]
    public float height;
    public LayerMask groundMask;
    public bool grounded;

    [Header("Terrain Sticking:")]
    public float maxGroundDistance = 3f;
    public CapsuleCollider playerCollider;
    private float groundNormal = 180f; //flat ground at default

    public Transform orientation;

    float horizontal_input;
    float vertical_input;

    Vector3 moveDirection;

    Rigidbody rb;

    //basic input
    private GlobalInput input;

    private void Start()
    {
        //initialize variables
        rb = GetComponent<Rigidbody>();

        //initialize a physics material
        PhysicsMaterial pm = new PhysicsMaterial();
        pm.dynamicFriction = 0.6f;
        pm.staticFriction = 0.6f;
        pm.bounciness = 0f;
        pm.frictionCombine = PhysicsMaterialCombine.Maximum;
        playerCollider.material = pm;
    }

    private void OnEnable()
    {
        //enable input
        input = new GlobalInput();
        input.Player.Move.Enable();
    }

    private void OnDisable()
    {
        //disable input
        input.Player.Move.Disable();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        CheckGrounded();
        StickToTerrain();
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

        //adjust for orientation
        moveDirection = orientation.forward * vertical_input + orientation.right * horizontal_input;
        Vector3 adjusted_movement = moveDirection.normalized * speed;

        //adjust for slope
        adjusted_movement *= SlopeMultiplier();

        //apply the movement as velocity
        Vector3 newVelocity = adjusted_movement;
        adjusted_movement.y = rb.linearVelocity.y;
        rb.linearVelocity = adjusted_movement;
    }

    //function used to keep the player stuck to the terrain
    private void StickToTerrain()
    {
        //dont perform if off ground or jumping
        if (!grounded)
        {
            return;
        }

        //we are going to raycast in front of the player to be predictive//
        float rayStartHeight = playerCollider.height / 2f;

        //cast from current position + offset
        Vector3 rayStart = transform.position - new Vector3(0f, rayStartHeight - 1f, 0f);

        //perform the raycast
        RaycastHit hit;
        if(Physics.Raycast(rayStart, Vector3.down, out hit, maxGroundDistance, groundMask))
        {
            //calculate where we should be
            float target = hit.point.y + rayStartHeight;
            float current = transform.position.y;
            float gap = current - target;

            //apply force proportional to the gap
            if(gap > 0.01f)
            {
                float force = gap * 100f;
                rb.AddForce(Vector3.down * force, ForceMode.Force);
            }

            //if we're close and falling then 0 velocity
            if(gap < 0.1f && rb.linearVelocity.y < 0)
            {
                Vector3 v = rb.linearVelocity;
                v.y = Mathf.Max(v.y, -1f);
                rb.linearVelocity = v;
            }

            //save the normal to be used for calculating the ground
            groundNormal = Vector3.Angle(hit.normal, Vector3.down);
        }
    }

    //function to find out whether or not we are grounded
    private void CheckGrounded()
    {
        float checkDistance = playerCollider.height / 2f + 1f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, groundMask))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    //function to calculate the slowdown multiplier
    private float SlopeMultiplier()
    {
        //only do if on the ground
        if (!grounded) return 1f;

        //get the slope angle between 0 and 90 (flat to wall)
        float slopeAngle = 180f - groundNormal;

        //if its too flat then don't even worry about it
        if (slopeAngle < 15f) return 1f;

        //determine if uphill/downhill
        Vector3 startOfRay = transform.position - new Vector3(0f, (playerCollider.height / 2f) - 1f, 0f);

        RaycastHit hit;
        if(Physics.Raycast(startOfRay, Vector3.down, out hit, maxGroundDistance, groundMask))
        {
            //this calculates the up vector of the slope's plane using its normal
            Vector3 slopeUp = Vector3.ProjectOnPlane(Vector3.up, hit.normal).normalized;

            //uses the dot product to get the angle between the move direction and up of the slope, which helps you know which way the player is facing
            //negative = downhill and positive = uphill
            float movingUphill = Vector3.Dot(moveDirection.normalized, slopeUp);

            //if its uphill (positive dot product)
            if (movingUphill > 0.1f)
            {
                //apply slowdown
                float slowdownFactor = Mathf.Clamp(slopeAngle-15, 0f, 45f) / 45f;

                return 1 - slowdownFactor;
            }
            //if downhill then don't change
            else if (movingUphill < -0.1f)
            {
                return 1f;
            }
        }
        return 1f; //no modification by default
    }
}
