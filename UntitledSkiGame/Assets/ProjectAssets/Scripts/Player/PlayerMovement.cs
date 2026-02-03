using Global_Input;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region singleton
    public static PlayerMovement Instance
    {
        get
        {
            return instance;
        }
    }

    private static PlayerMovement instance = null;
    #endregion singleton

    private void Awake()
    {
        //initialize the singleton
        if(instance)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    //basic input
    private GlobalInput input;

    private void OnEnable()
    {
        //initialize movement
        input = new GlobalInput();

        //enable any movement used in this script
    }
}
