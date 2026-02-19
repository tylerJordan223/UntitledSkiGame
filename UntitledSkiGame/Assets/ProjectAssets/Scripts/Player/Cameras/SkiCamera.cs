using Global_Input;
using UnityEngine;
using UnityEngine.Animations;

public class SkiCamera : MonoBehaviour
{
    [Header("Object References")]
    public Transform orientation;
    public Transform player;
    public Transform player_obj;
    public Rigidbody rb;

    [Range(0f, 100f)]
    public float rotation_speed = 50f;

    //input
    private GlobalInput input;

    private void Start()
    {
        //lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        input = new GlobalInput();
        input.Mounted.Enable();
    }

    private void OnDisable()
    {
        input.Mounted.Disable();
    }

    private void Update()
    {
        float rotation = input.Mounted.Rotate.ReadValue<float>() * 10;

        if(rotation != 0)
        {
            player.Rotate(0, rotation * rotation_speed * Time.deltaTime, 0);
        }
    }
}
