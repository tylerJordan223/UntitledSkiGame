using Autodesk.Fbx;
using Global_Input;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.LookDev;

public class SkiCamera : MonoBehaviour
{
    [Header("Object References")]
    public Transform orientation;
    public Transform player;
    public Transform player_obj;
    public Rigidbody rb;
    public GameObject skis;

    [Range(0f, 100f)]
    public float rotation_speed = 20f;

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

            //handle ski rotation
            foreach(Transform ski in skis.transform.GetComponentsInChildren<Transform>())
            {
                //Debug.Log("Local Rotation: " + ski.localRotation.y + "\nGlobal Rotation: " + ski.rotation.y);

                if(rotation < 0)
                {
                    //turning left
                    if(ski.localRotation.y > -0.20)
                    {
                        ski.Rotate(0, rotation * rotation_speed * Time.deltaTime, 0f, Space.Self);
                    }
                }
                else 
                {
                    //turning right
                    if (ski.localRotation.y < 0.20)
                    {
                        ski.Rotate(0, rotation * rotation_speed * Time.deltaTime, 0f, Space.Self);
                    }
                }
            }
        }
        else
        {
            //straighten out skis if necessary
            foreach (Transform ski in skis.transform.GetComponentsInChildren<Transform>())
            {
                //straighten out the kis by multiplying them by the negative of hteir current rotation
                if (Mathf.Abs(ski.localRotation.y) >= 0.01)
                {
                    ski.Rotate(0, 25f * -Mathf.Sign(ski.localRotation.y) * Time.deltaTime, 0, Space.Self);
                }
            }
        }

        //set the ski z rotation to the slope angle
        skis.transform.localRotation = Quaternion.Euler(0f, 0f, SkiMovement.Instance.slopeAngle * Mathf.Sign(SkiMovement.Instance.uphill));
    }
}
