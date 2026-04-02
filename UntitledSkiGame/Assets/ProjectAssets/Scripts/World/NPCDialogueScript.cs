using Global_Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogueScript : MonoBehaviour
{
    //get the player objects
    [SerializeField] private GameObject fake_player;
    [SerializeField] private GameObject npc_body;
    [SerializeField] private GameObject cam;

    //other objects
    [SerializeField] private GameObject interact_alert;

    //trigger bool
    private bool can_interact;

    //game input
    private GlobalInput input;

    //canvas object recieved later
    private GameObject dialogue_canvas;

    private void Start()
    {
        //disable the fake player initially
        fake_player.SetActive(false);
        interact_alert.SetActive(false);
        cam.SetActive(false);

        //only triggered by player entering
        can_interact = false;
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.Mounted.Interact.performed += StartDialogue;
        input.Mounted.Interact.Enable();
    }

    private void OnDisable()
    {
        input.Mounted.Interact.Disable();
    }

    private void StartDialogue(InputAction.CallbackContext context)
    {
        if(can_interact)
        {
            //enable the fake player and activate dialogue
            fake_player.SetActive(true);
            interact_alert.SetActive(false);
            cam.SetActive(true);
            //also returns canvas to be used after
            dialogue_canvas = GameManager.instance.EnableNPCDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            can_interact = true;
            interact_alert.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            can_interact = false;
            interact_alert.SetActive(false);
        }
    }
}
