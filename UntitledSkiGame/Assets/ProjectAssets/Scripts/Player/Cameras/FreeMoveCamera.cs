using Global_Input;
using System;
using System.Collections;
using UnityEngine;

public class FreeMoveCamera : MonoBehaviour
{
    [Header("Object References")]
    public Transform orientation;
    public Transform player;
    public Transform player_obj;
    public Rigidbody rb;

    public float rotation_speed;

    //input
    private GlobalInput input;

    //movement check
    private bool can_control;

    private void Start()
    {
        //lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        can_control = true;
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.Unmounted.Enable();

        StartCoroutine(TransitionPeriod());
    }

    private void OnDisable()
    {
        input.Unmounted.Disable();
        can_control = false;
    }

    private void Update()
    {
        if(can_control)
        {
            //rotation the player's orientation//

            //ignoring the y by removing it entirely
            Vector3 dir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = dir.normalized;

            //getting the movement from the player
            Vector2 movement = input.Unmounted.Move.ReadValue<Vector2>();
            //**NOTE: The movement base input reads up/down or forward/backward as its first value (x) and left/right as its second value (y)
            float horizontal_movement = -movement.y;
            float vertical_movement = movement.x;

            //math to calculate input direction
            // move each of the player direction vectors forward by multiplying them by the respective movement value
            Vector3 input_dir = orientation.forward * vertical_movement + orientation.right * horizontal_movement;

            if (input_dir != Vector3.zero)
            {
                player_obj.forward = Vector3.Slerp(player_obj.forward, input_dir.normalized, Time.deltaTime * rotation_speed);
            }
        }
    }


    //function used to force a wait before activating movement
    private IEnumerator TransitionPeriod()
    {
        yield return new WaitForSeconds(2.5f);
        can_control = true;
    }
}
