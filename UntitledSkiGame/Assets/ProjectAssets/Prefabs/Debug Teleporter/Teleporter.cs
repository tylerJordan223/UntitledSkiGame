using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject reciever;
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision detected with " + other.name);
        other.transform.parent.position = reciever.transform.position;

    }
}
