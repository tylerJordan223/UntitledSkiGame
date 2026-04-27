using UnityEngine;

public class AlignmentScript : MonoBehaviour
{
    //script that has the whole purpose of keeping the object its on aligned with the object its given//

    [SerializeField] Transform aligned_object;
    public bool position;
    public bool rotation;

    [Header("rotation axes")]
    public bool x;
    public bool y;
    public bool z;

    private void LateUpdate()
    {
        //make sure this only is active if enabled
        if(aligned_object.gameObject.activeSelf && aligned_object.parent.gameObject.activeSelf)
        {
            //keep rotation aligned
            if (rotation)
            {
                Quaternion new_rot = transform.localRotation;

                if(x)
                {
                    new_rot.x = aligned_object.localRotation.x;
                }
                if(y)
                {
                    new_rot.y = aligned_object.localRotation.y;
                }
                if(z)
                {
                    new_rot.z = aligned_object.localRotation.z;
                }

                transform.localRotation = new_rot;
            }

            //keep position aligned
            if (position)
            {
                transform.position = aligned_object.position;
            }
        }
    }
}
