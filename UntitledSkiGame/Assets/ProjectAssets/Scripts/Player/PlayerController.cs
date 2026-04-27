using Global_Input;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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

    //boolean used to discern between movement types
    bool skiing;
    bool swapping;
    private void Start()
    {
        skiing = false;
        swapping = false;
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.UI.Submit.performed += SwapControls;
        input.UI.Submit.Enable();
    }

    private void OnDisable()
    {
        input.UI.Submit.Disable();
    }

    //function used to swap between types of movement
    private void SwapControls(InputAction.CallbackContext context)
    {
        if(!swapping)
        {
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
}
