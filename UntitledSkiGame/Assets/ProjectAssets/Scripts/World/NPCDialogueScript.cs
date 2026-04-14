using Global_Input;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private TextMeshProUGUI dialogue_text;
    public float talkSpeed;

    //dialogue flags/details
    [SerializeField] private List<string> dialogue_list;
    private int current_dialogue_line;
    private bool printing;
    private bool skip;

    private void Start()
    {
        //disable the fake player initially
        fake_player.SetActive(false);
        interact_alert.SetActive(false);
        cam.SetActive(false);

        //only triggered by player entering
        can_interact = false;
        printing = false;
        skip = false;
        current_dialogue_line = -1;
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.Mounted.Interact.performed += StartDialogue;
        input.Mounted.Interact.Enable();

        input.Mounted.Push.performed += SkipCheck;
        input.Mounted.Push.Enable();
    }

    private void OnDisable()
    {
        input.Mounted.Interact.Disable();
        input.Mounted.Push.Disable();
    }

    private void StartDialogue(InputAction.CallbackContext context)
    {
        if(can_interact)
        {
            //enable the fake player and activate dialogue
            fake_player.SetActive(true);
            interact_alert.SetActive(false);
            cam.SetActive(true);
            can_interact = false;

            //also returns canvas to be used after
            dialogue_text = GameManager.instance.EnableNPCDialogue();

            current_dialogue_line = 0;
            StartCoroutine(PrintDialogue(dialogue_list[current_dialogue_line]));
        }
    }

    private void EndDialogue()
    {
        fake_player.SetActive(false);
        cam.SetActive(false);
        GameManager.instance.DisableNPCDialogue();
    }

    //actual dialogue code
    private IEnumerator PrintDialogue(string text)
    {
        //currently printing
        printing = true;

        //make the textbox blank
        dialogue_text.text = "";

        //loop through every character of the string
        for(int i = 0; i < text.Length; i++)
        {
            //add a character
            dialogue_text.text = dialogue_text.text + text[i];
            //time between characters
            yield return new WaitForSeconds(talkSpeed);

            //if you skip the dialogue print the whole thing
            if(skip)
            {
                dialogue_text.text = text;
                break;
            }
        }

        yield return new WaitForSeconds(0.1f);

        if(skip)
        {
            skip = false;
        }

        printing = false;
    }

    //function to skip dialogue if pressed
    private void SkipCheck(InputAction.CallbackContext context)
    {
        //to actually skip during printing
        if(!skip && printing)
        {
            skip = true;
        }

        //function to move between dialogue bits
        if(!printing && dialogue_text.enabled)
        {
            current_dialogue_line += 1;

            //if theres more to say
            if(current_dialogue_line < dialogue_list.Count)
            {
                StartCoroutine(PrintDialogue(dialogue_list[current_dialogue_line]));
            }
            else
            {
                EndDialogue();
            }
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
