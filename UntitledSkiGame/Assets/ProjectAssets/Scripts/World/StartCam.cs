using UnityEngine;

public class StartCam : MonoBehaviour
{
    //the only purpose of this class is to disable the camera on start!

    private void Awake()
    {
        this.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
