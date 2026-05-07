using UnityEngine;

public class TrickAnimFunctions : MonoBehaviour
{
    //this script contains functions to allow the player animator to use them//

    public void CallEnableSkis()
    {
        PlayerController.instance.EnableSkis();
    }

    public void CallDisableSkis()
    {
        PlayerController.instance.DisableSkis();
    }
}
