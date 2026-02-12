using Global_Input;
using UnityEngine;

public class PlayerMovementOld : MonoBehaviour
{
    #region singleton
    public static PlayerMovementOld Instance
    {
        get
        {
            return instance;
        }
    }

    private static PlayerMovementOld instance = null;
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
    public float maxSlopeAngle = 45f;
    public float slopeForce = 8f;

    [Header("Grounding:")]
    public float height;
    public LayerMask groundMask;
    public bool grounded;

    [Header("Terrain Sticking:")]
    public float maxGroundDistance = 1.5f;
    public float snapDistanceMin = 0.5f;
    public CapsuleCollider playerCollider;
    private Vector3 groundNormal;

    public Transform orientation;

    float horizontal_input;
    float vertical_input;

    Vector3 moveDirection;

    Rigidbody rb;

    //basic input
    private GlobalInput input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = speed;
    }

    private void OnEnable()
    {
        //initialize movement
        input = new GlobalInput();

        //enable any movement used in this script
        input.Player.Move.Enable();
    }

    private void Update()
    {
        //handle the drag
        if (grounded)
        {
            rb.linearDamping = ground_drag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }

    private void FixedUpdate()
    {
        //handle movement then handle terrain
        MovePlayer();
        CheckGrounded();
        TerrainStick();

        //apply a constnat force down to stay grounded
        rb.AddForce(Vector3.down * 10f, ForceMode.Force);
    }

    //function used to move the player
    private void MovePlayer()
    {
        //get the movement direction from input
        Vector2 movement = input.Player.Move.ReadValue<Vector2>();
        //set variables to equal proper values
        horizontal_input = movement.x;
        vertical_input = movement.y;

        //determine the movement direction
        moveDirection = orientation.forward * vertical_input + orientation.right * horizontal_input;

        //adjust based on velocity
        Vector3 targetVelocity = moveDirection.normalized * speed;

        //get the current velocity
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //lerp to make smoother
        float acceleration = 20f;
        Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration);

        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;

        //apply slope resistance if needed
        ApplySlopeResistance(moveDirection);
    }

    //function to handle staying stuck to the terrain
    private void TerrainStick()
    {
        //only stick if grounded and not jumping
        if (!grounded || rb.linearVelocity.y > 0.5f)
        {
            return;
        }

        //raycast from the bottom of collider player downward
        float rayStartHeight = playerCollider.height / 2f;
        Vector3 startOfRay = transform.position - new Vector3(0, rayStartHeight - 0.1f, 0f);

        //perform the raycast downwards
        RaycastHit hit;
        if (Physics.Raycast(startOfRay, Vector3.down, out hit, maxGroundDistance, groundMask))
        {
            //calculate distance to ground
            float distanceToGround = hit.distance;

            //if theres a gap then snap down
            if (distanceToGround > 0.05f)
            {
                //get target position
                float target = hit.point.y + rayStartHeight;

                //move to position
                Vector3 newPos = transform.position;
                newPos.y = target;
                rb.MovePosition(newPos);
            }

            //zero out downward velocity for no bouncing
            if (rb.linearVelocity.y < 0)
            {
                Vector3 v = rb.linearVelocity;
                v.y = 0;
                rb.linearVelocity = v;
            }
        }
    }

    private void CheckGrounded()
    {
        float checkDistance = playerCollider.height / 2f + 0.2f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, groundMask))
        {
            grounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            grounded = false;
            groundNormal = Vector3.up;
        }
    }

    //helper function to calculate slope resistance
    private void ApplySlopeResistance(Vector3 moveDirection)
    {
        //only do this if you're on the ground and moving a true speed
        if (!grounded || moveDirection.magnitude < 0.1f)
        {
            return;
        }

        //get the angle of the slope
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        //if its steep
        if (slopeAngle > maxSlopeAngle)
        {
            //check if trying to move up
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.up, groundNormal);
            float movingUpSlope = Vector3.Dot(moveDirection, -slopeDirection);

            if (movingUpSlope > 0) //moving up
            {
                float slopeFactor = (slopeAngle - maxSlopeAngle) / (90f - maxSlopeAngle);
                Vector3 counterForce = -moveDirection * slopeFactor * slopeForce;
                rb.AddForce(counterForce, ForceMode.Force);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerCollider == null)
            return;

        // Draw ground check ray
        Gizmos.color = grounded ? Color.green : Color.red;
        float checkDistance = playerCollider.height / 2f + 0.2f;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);

        // Draw stick ray
        float rayStartHeight = playerCollider.height / 2f;
        Vector3 rayStart = transform.position - new Vector3(0, rayStartHeight - 0.1f, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * snapDistanceMin);

        // Draw ground normal
        if (grounded)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + groundNormal * 2f);
        }
    }
}
