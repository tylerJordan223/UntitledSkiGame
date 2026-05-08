using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    //quest information
    [Header("Quest Information")]
    public string title;
    public string description;

    //type of quest that it is
    [Header("Type of Quest:")]
    public bool trick;
    public bool time;
    public bool find;

    //state checks
    [Header("Special Cases:")]
    public bool can_redo;
    public bool completed;

    //variables to hold the time
    [Header("Requirements: ")]
    public float best_time;
    public float goal_time;

    //variables to hold the trick animations
    [Header("Records: ")]
    public float best_score;
    public float goal_score;

    //the function that runs when you start the quest
    public void StartQuest()
    {
        //enable options
        QuestSystem.instance.enableEndArea(this);
        QuestSystem.instance.enableObjectHolder(this);

        //start quest
        QuestSystem.instance.SetActiveQuest(this);
    }

    public void EndQuest()
    {
        QuestSystem.instance.disableEndArea(this);
        QuestSystem.instance.CompleteQuest(this);
        completed = true;
    }
}
