using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    //type of quest that it is
    public bool trick;
    public bool time;
    public bool find;

    //variables to hold the time
    public float best_time;
    public float goal_time;

    //variables to hold the trick animations
    public float best_score;
    public float goal_score;

    //for the object that will hold any objects that are only on during quest
    public GameObject quest_objects;

    //locations for the player
    public Transform startLocation;
    public BoxCollider endLocation; //used for time/trick

    public void StartQuest()
    {
        Debug.Log("STARTED QUEST");
    }

    public void EndQuest()
    {
        Debug.Log("ENDED QUEST");
    }
}
