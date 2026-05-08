using Global_Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    //quest details
    [SerializeField] Quest my_quest;

    //dialogue flags/details
    private List<string> current_dialogue;
    [SerializeField] private List<string> before_dialogue_list;
    [SerializeField] private List<string> during_dialogue_list;
    [SerializeField] private List<string> after_dialogue_list;

    private int current_dialogue_line;
    private bool printing;
    private bool skip;

    //functions for yes/no
    [SerializeField] public UnityEvent yes_action;
    [SerializeField] public UnityEvent no_action;

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
            input.Mounted.Push.Enable();

            //also returns canvas to be used after
            dialogue_text = GameManager.instance.EnableNPCDialogue();

            Debug.Log($"Quest is active: {QuestSystem.instance.active_quests.Contains(my_quest)}");
            Debug.Log($"Quest is completed: {my_quest.completed}");

            //decide on which dialogue you're using
            if(!my_quest.completed && !QuestSystem.instance.active_quests.Contains(my_quest))
            {
                //if incomplete and not started
                current_dialogue = before_dialogue_list;
            }else if(QuestSystem.instance.active_quests.Contains(my_quest))
            {
                //if started
                current_dialogue = during_dialogue_list;
            }
            else
            {
                current_dialogue = after_dialogue_list;
            }

            //check to see if there is already an active quest going on//
            if (QuestSystem.instance.counter_active)
            {
                current_dialogue = during_dialogue_list;
            }

            current_dialogue_line = 0;
            StartCoroutine(PrintDialogue(current_dialogue[current_dialogue_line]));
        }
    }

    public void EndDialogue()
    {
        input.Mounted.Push.Disable();
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
            //if theres more to say
            current_dialogue_line += 1;
            if(current_dialogue_line < current_dialogue.Count)
            {
                StartCoroutine(PrintDialogue(current_dialogue[current_dialogue_line]));
            }
            else
            {
                //check to see if there is already an active quest going on//
                if (QuestSystem.instance.counter_active)
                {
                    EndDialogue();
                    return;
                }

                //function that builds the choice menu
                //will only pop up menu if not done or you're able to redo it
                if( ( !my_quest.completed && !QuestSystem.instance.active_quests.Contains(my_quest) ) || ( my_quest.completed && my_quest.can_redo && !QuestSystem.instance.active_quests.Contains(my_quest)))
                {
                    GameManager.instance.OnEnableChoiceMenu(my_quest, yes_action, no_action);
                }
                else
                {
                    //just close dialogue
                    EndDialogue();
                }
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
