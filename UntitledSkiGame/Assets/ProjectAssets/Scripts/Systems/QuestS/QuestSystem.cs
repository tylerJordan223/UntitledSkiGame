using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance;

    private void Awake()
    {
        if(instance)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    [SerializeField] List<Quest> quests;

    //lists that contain the objects with reference to quests
    public Dictionary<Quest, GameObject> endAreas = new Dictionary<Quest, GameObject>();
    public Dictionary<Quest, GameObject> objectHolders = new Dictionary<Quest, GameObject>();

    public List<Quest> active_quests;

    private void Start()
    {
        //add the unfinished quests to start
        foreach(Quest q in quests)
        {
            q.completed = false;
        }
    }

    //function to set this quest as the active quest
    public void SetActiveQuest(Quest q)
    {
        if(!active_quests.Contains(q))
        {
            active_quests.Add(q);
        }
    }

    //function to check if a quest is finished
    public bool checkIsFinished(Quest q)
    {
        return q.completed;
    }

    //function that adds end areas to the proper dictionary
    public void addEndArea(Quest q, GameObject go)
    {
        endAreas[q] = go;
    }

    //function that adds object holders to the proper dictionary
    public void addObjectHolder(Quest q, GameObject go)
    {
        objectHolders[q] = go;
    }

    //function to run when a quest is completed
    public void CompleteQuest(Quest q)
    {
        active_quests.Remove(q);

        //disable the areas too
        disableEndArea(q);
        disableObjectHolder(q);
    }

    //funictions to enable/disable quest object holders and end areas

    public void enableEndArea(Quest q)
    {
        endAreas[q].SetActive(true);
    }
    public void disableEndArea(Quest q)
    {
        endAreas[q].SetActive(false);
    }
    public void enableObjectHolder(Quest q)
    {
        objectHolders[q].SetActive(true);
    }
    public void disableObjectHolder(Quest q)
    {
        objectHolders[q].SetActive(false);
    }

}
