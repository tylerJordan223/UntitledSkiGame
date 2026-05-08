using System.Collections;
using UnityEngine;

public class StartCam : MonoBehaviour
{
    //the only purpose of this class is to disable the camera on start!

    private void Awake()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(CamTrans());
    }

    private IEnumerator CamTrans()
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.SetActive(false);
    }
}
