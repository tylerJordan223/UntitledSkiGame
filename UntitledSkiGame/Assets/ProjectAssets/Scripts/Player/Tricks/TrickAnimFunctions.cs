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

    public void PlaySTEP()
    {
        if(PlayerMovement.Instance.on_ice)
        {
            AudioManager.instance.PlaySFXPitched(AudioManager.instance.step_ice, Random.Range(0.5f, 1.5f));
        }
        else
        {
            AudioManager.instance.PlaySFXPitched(AudioManager.instance.step_snow, Random.Range(0.5f, 1.5f));
        }
    }

    public void PlayPUSH()
    {
        if(SkiMovement.Instance.on_ice)
        {
            AudioManager.instance.PlaySFXPitched(AudioManager.instance.push_ice, Random.Range(0.5f, 1.5f));
        }
        else
        {
            AudioManager.instance.PlaySFXPitched(AudioManager.instance.push, Random.Range(0.5f, 1.5f));
        }
    }
}
