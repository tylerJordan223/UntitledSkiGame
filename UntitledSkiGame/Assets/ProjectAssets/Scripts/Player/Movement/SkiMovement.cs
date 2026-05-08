using Global_Input;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

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
    public float max_speed;
    public float ground_drag;
    public float slopeAcceleration;
    public float playerAcceleration = 0;

    //ski timer
    private float ski_timer;
    public float ski_cooldown;

    public Transform orientation;

    [Header("Grounding:")]
    public float height;
    public LayerMask groundMask;
    public bool grounded;

    [Header("Gravity/Acceleration:")]
    public float maxGroundDistance = 0.5f;
    public float maxSlopeAngle = 45;
    public CapsuleCollider playerCollider;

    [Header("Ramp Trick Detection")]
    public bool isOnRamp;
    public bool canDoRampTricks;

    //saved values for calculation
    public float slopeAngle = 180f; //flat ground at default
    public int uphill;
    private Vector3 groundNormal;
    public Rigidbody rb;
    public bool on_ice;

    //animation information
    [Header("Animation:")]
    public Animator anim;

    [Header("AUDIO:")]
    public AudioSource windGenerator;
    public AudioSource ski_loop;
    public AudioClip ski_snow;
    public AudioClip ski_ice;

    //movement values
    private Vector3 moveDirection;
    private float horizontal_input;
    private float vertical_input;

    //basic input
    private GlobalInput input;

    // ramp tracking
    private bool wasGroundedLastFrame;
    private bool wasOnRampLastFrame;

    private void Start()
    {
        //initialize variables
        rb = GetComponent<Rigidbody>();
        on_ice = false;
    }

    private void OnEnable()
    {
        //enable input
        input = new GlobalInput();
        input.Mounted.Push.performed += Push;
        input.Mounted.Enable();

        //remove player velocity
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
        }
        playerAcceleration = 0f;

        //initialize timer
        ski_timer = ski_cooldown;

        wasGroundedLastFrame = grounded;
        wasOnRampLastFrame = false;
        isOnRamp = false;
        canDoRampTricks = false;
    }

    private void OnDisable()
    {
        //disable input
        input.Mounted.Disable();

        //mute the audio
        windGenerator.volume = 0;
        ski_loop.volume = 0;
    }

    private void FixedUpdate()
    {
        bool groundedBeforeCheck = grounded;
        bool onRampBeforeCheck = isOnRamp;

        //only check for the groundedness if you are still grounded (this gets reset on collision
        if (grounded)
        {
            CheckGrounded();
        }

        // if we just left the ground and we were on a ramp, allow trick window
        if (groundedBeforeCheck && !grounded && onRampBeforeCheck)
        {
            canDoRampTricks = true;
        }

        // landing ends the ramp-air trick window
        if (!groundedBeforeCheck && grounded)
        {
            canDoRampTricks = false;
        }

        wasGroundedLastFrame = grounded;
        wasOnRampLastFrame = isOnRamp;

        //always move/apply the gravity
        MovePlayer();
        ApplyGravity();
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

        //shrink timer
        if(ski_timer > 0)
        {
            ski_timer -= Time.deltaTime;
        }

        //start by shrinking the starting wind time
        float wind_volume_goal = playerAcceleration - 0.5f;
        wind_volume_goal *= AudioManager.instance.sfx_volume; //multiply by current maxVolume to lerp properly

        //handle the audio//
        if(windGenerator.volume != wind_volume_goal)
        {
            windGenerator.volume = Mathf.Lerp(windGenerator.volume, wind_volume_goal, Time.deltaTime * 10f);
        }

        //SKI SOUNDS//
        //only do if grounded
        if(grounded)
        {
            if(!ski_loop.isPlaying)
            {
                ski_loop.Play();
            }

            //swap between ice and snow
            if(on_ice)
            {
                if(ski_loop.clip != ski_ice)
                {
                    ski_loop.Stop();
                    ski_loop.clip = ski_ice;
                    ski_loop.Play();
                }
            }
            else
            {
                if(ski_loop.clip != ski_snow)
                {
                    ski_loop.Stop();
                    ski_loop.clip = ski_snow;
                    ski_loop.Play();
                }
            }

            //mimic accerlation exactly
            float ski_volume_goal = playerAcceleration;
            ski_volume_goal *= AudioManager.instance.sfx_volume; //multiply by current maxVolume to lerp properly

            //handle the audio//
            if (ski_loop.volume != ski_volume_goal)
            {
                ski_loop.volume = Mathf.Lerp(ski_loop.volume, ski_volume_goal, Time.deltaTime * 10f);
            }
        }
        else
        {
            ski_loop.Stop();
        }
    }

    //function used to move the player
    private void MovePlayer()
    {
        //calculating the initial movement direction
        Vector3 inputDirection = orientation.right;

        //important case for actually doing this
        if (grounded && inputDirection.magnitude > 0.1f && playerAcceleration > 0f)
        {
            //project that movement onto the slope
            moveDirection = Vector3.ProjectOnPlane(inputDirection, groundNormal);

            //apply speed
            Vector3 adjusted_movement = moveDirection * max_speed * playerAcceleration;

            //transition to the goal velocity
            Vector3 current = rb.linearVelocity;
            Vector3 change = adjusted_movement - current;
            change.y = 0; //don't affect vertical velocity

            //apply acceleration
            rb.AddForce(change * 10f, ForceMode.Acceleration);
        }

        //alter acceleration based on angle, can happen when 0 (on angled slope)
        if (grounded && !input.Mounted.Brake.IsPressed() && playerAcceleration != 0)
        {
            //slop angle should be between 0 (flat ground) and 45 (max angle)
            //add to the player acceleration based on angle multiplied by the slope
            playerAcceleration -= uphill * ((slopeAngle / 45f) * 0.5f * Time.deltaTime);
        }
        else if (grounded && input.Mounted.Brake.IsPressed())
        {
            //this is braking, happens instead of thet slope speed up or slow down
            playerAcceleration -= 0.4f * Time.deltaTime;
        }
        //clamp to not go over or under acceleration
        playerAcceleration = Mathf.Clamp(playerAcceleration, 0f, 1.0f);

        //if flat ground slow down anyways if on flat ground
        if (playerAcceleration > 0f && slopeAngle == 0 && !on_ice)
        {
            playerAcceleration -= 0.1f * Time.deltaTime;
        }
    }

    //applying gravity
    private void ApplyGravity()
    {
        if (!grounded)
        {
            //normal gravity in air
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);
        }
        else
        {
            if (slopeAngle > 1f)
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
            isOnRamp = IsRampObject(hit.collider.transform);

            //check to see if you're on ice
            if (hit.collider.CompareTag("ice"))
            {
                on_ice = true;
            }
            else
            {
                on_ice = false;
            }

            //get the slope angle
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= maxSlopeAngle)
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

            //additionally find the uphill/downhill value
            Vector3 slopeUp = Vector3.ProjectOnPlane(Vector3.up, hit.normal).normalized;
            
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                uphill = (int)(1 * Mathf.Sign(Vector3.Dot(moveDirection.normalized, slopeUp)));
            }
            else
            {
                uphill = 0;
            }
        }
        else
        {
            //not on the ground
            grounded = false;
            groundNormal = Vector3.up;
            isOnRamp = false;
        }
    }

    private bool IsRampObject(Transform hitTransform)
    {
        if (hitTransform == null) return false;

        Transform current = hitTransform;
        while (current != null)
        {
            string objectName = current.name.ToLower();

            if (objectName.Contains("ramp"))
                return true;

            if (objectName.Contains("ramps"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void Push(InputAction.CallbackContext context)
    {

        //store the kit for checking
        RaycastHit hit;

        //check if theres a wall straight ahead of the player, can't push if thats the case
        if (Physics.Raycast(playerCollider.transform.position, playerCollider.transform.right, out hit, 1f))
        {
            if (hit.transform.CompareTag("Obstacle"))
            {
                //end the function here, no reason to gain acceleration when facing a wall
                return;
            }
        }

        //only push if under a certain threshold and timer is good
        if (playerAcceleration < 0.7f && ski_timer <= 0)
        {
            StartCoroutine(PerformPush());
        }
    }


    private IEnumerator PerformPush()
    {
        //set up the timer
        ski_timer = ski_cooldown;

        //animate the ski push
        anim.SetTrigger("push");

        //12 frames into the animation at 60fps
        yield return new WaitForSeconds(0.5f);

        playerAcceleration += 0.2f;
    }

    //function that runs when colliding with an obstacle
    private void HitObstacle(Vector3 p)
    {
        //start by finding the angle between the player's forward and the vector from the player to the obstacle
        float angle_between = Vector3.Angle(playerCollider.transform.right.normalized, (p - playerCollider.transform.position).normalized);

        //need to be going fast enough / hit it at a hard enough angle
        if (angle_between <= 60 && playerAcceleration > 0.3f)
        {
            //rotate to test, good enough !!!for now!!!
            transform.Rotate(0, 180-angle_between, 0);

            //decellerate depending on value
            playerAcceleration /= (2 * (playerAcceleration + 1));
        }
        else
        {
            //straighten out against wall
            if(angle_between <= 90 && playerAcceleration > 0.3f)
            {
                transform.Rotate(0, 90 - angle_between, 0);
            }
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

    private void OnCollisionEnter(Collision collision)
    {
        //if its an obstacle && has to check whether the script is on or not
        if(collision.gameObject.CompareTag("Obstacle") && enabled)
        {
            //runs the function using the point of collision
            HitObstacle(collision.contacts[0].point);
        }
    }
}
