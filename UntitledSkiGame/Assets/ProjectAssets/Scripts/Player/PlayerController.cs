using Global_Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    //singleton
    private void Awake()
    {
        if(instance)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    //this is a script that is used to swap between different player movements//

    GlobalInput input;

    [Header("Skiing Objects")]
    public SkiMovement ski_movement;
    public GameObject ski_camera;
    public GameObject skis;

    [Header("Walking Objects")]
    public PlayerMovement walking_movement;
    public GameObject walking_camera;

    [Header("Other Objects")]
    public GameObject orientation;
    public GameObject body;

    [Header("Animator")]
    [SerializeField] Animator anim;

    [Header("Ragdoll")]
    [SerializeField] GameObject ragdoll;
    [SerializeField] Rigidbody ragdollPelvis;
    [SerializeField] GameObject ragdollCamera;
    private Transform player_respawn_point;
    public bool ragdolling;
    private Rigidbody[] ragdoll_rbs;
    private Dictionary<Transform, RagdollTransform> initialTransforms = new Dictionary<Transform, RagdollTransform>();

    //boolean used to discern between movement types
    public bool skiing;
    public bool swapping;
    private void Start()
    {
        skiing = false;
        swapping = false;

        //get all the ragdoll information//
        ragdoll_rbs = ragdoll.GetComponentsInChildren<Rigidbody>();
        //create new trasnform locally for each of the bones
        foreach(Rigidbody rb in ragdoll_rbs)
        {
            RagdollTransform new_t = new RagdollTransform();
            new_t.Pos = rb.transform.localPosition;
            new_t.Rot = rb.transform.localRotation;
            initialTransforms[rb.transform] = new_t;
        }
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.UI.Submit.performed += SwapControls;
        input.UI.Submit.Enable();

        input.Mounted.Push.performed += RespawnPlayer;
        input.Mounted.Push.Enable();
    }

    private void OnDisable()
    {
        input.UI.Submit.Disable();
        input.Mounted.Push.Disable();
    }

    //function used to swap between types of movement
    private void SwapControls(InputAction.CallbackContext context)
    {
        if(!swapping)
        {
            //check for ski speed check
            if(skiing)
            {
                if(ski_movement.playerAcceleration >= 0.1f)
                {
                    //break and do not transition
                    return;
                }
            }

            //run the swap coroutine
            swapping = true;

            //swap the animation
            anim.SetBool("do_ski", !anim.GetBool("do_ski"));

            StartCoroutine(PerformSwap());
        }
    }

    private IEnumerator PerformSwap()
    {
        //swap to walking, only do it if you're slow enough
        if (skiing && ski_movement.playerAcceleration < 0.1f)
        {
            //disable ski things
            ski_camera.SetActive(false);
            skis.SetActive(false);
            ski_movement.enabled = false;

            //enable walking things
            walking_camera.SetActive(true);
            yield return new WaitForSeconds(2.5f); //wait for everythign to catch up
            walking_movement.enabled = true;

            skiing = false;
        }
        else if(!skiing) //swap to skiing
        {
            //disable walking things
            walking_camera.SetActive(false);
            walking_movement.enabled = false;

            //enable ski things
            ski_camera.SetActive(true);
            skis.SetActive(true);
            yield return new WaitForSeconds(2.5f); //wait for everythign to catch up
            ski_movement.enabled = true;

            //make orientation forward based on player 
            orientation.transform.forward = body.transform.forward;

            skiing = true;
        }

        //make it so you can swap again
        swapping = false;
    }

    //function to trigger the ragdoll
    public void TriggerRagdoll()
    {

        //enable ragdoll and spawn it where the player is
        ragdoll.SetActive(true);
        ragdoll.transform.position = body.transform.position;
        ragdollPelvis.linearVelocity = SkiMovement.Instance.rb.linearVelocity * 10;
        ragdollPelvis.angularVelocity = SkiMovement.Instance.rb.angularVelocity * 10;

        //reset the velocity
        SkiMovement.Instance.rb.linearVelocity = Vector3.zero;
        SkiMovement.Instance.rb.angularVelocity = Vector3.zero;
        SkiMovement.Instance.playerAcceleration = 0f; // reset speed just in case

        //swap cameras
        ragdollCamera.SetActive(true);
        ragdollCamera.transform.position = ski_camera.transform.position;
        ragdollCamera.transform.rotation = ski_camera.transform.rotation;
        ski_camera.SetActive(false);        

        //get the respawn point set
        player_respawn_point = this.transform;

        //disable the playerMovement
        ski_movement.enabled = false;

        //disable player body
        body.SetActive(false);

        ragdolling = true;
    }

    public void RespawnPlayer(InputAction.CallbackContext context)
    {
        if(ragdolling)
        {
            //reset the ragdoll and respawn it
            //RESET//
            ResetRagdoll();
            ragdoll.SetActive(false);
            ragdollCamera.SetActive(false);

            //re-enable player movement
            ski_movement.enabled = true;

            body.SetActive(true);
            SkiMovement.Instance.anim.SetBool("do_ski", true); //have to re-set this because its disabled when the body is disabled
            transform.position = player_respawn_point.position;

            SkiMovement.Instance.playerAcceleration = 0f; // reset speed just in case

            //re enable camera
            ski_camera.SetActive(true);

            //update info
            ragdolling = false;
        }
    }

    private void ResetRagdoll()
    {
        //do this to all bones
        foreach(Rigidbody rb in ragdoll_rbs)
        {
            //stop them 
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            
            if(initialTransforms.TryGetValue(rb.transform, out RagdollTransform saved))
            {
                rb.transform.localPosition = saved.Pos;
                rb.transform.localRotation = saved.Rot;
            }

            //undo kineticness
            rb.isKinematic = false;
        }
    }


    //disable and enable the skis visually for tricks
    public void DisableSkis()
    {
        skis.SetActive(false);
    }

    public void EnableSkis()
    {
        skis.SetActive(true);
    }
}

//object to help with ragdoll reset
public class RagdollTransform
{
    public Vector3 Pos;
    public Quaternion Rot;
}
