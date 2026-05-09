using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Quest Canvas")]
    [SerializeField] Canvas quest_canvas;
    [SerializeField] TextMeshProUGUI CounterTitle;
    [SerializeField] TextMeshProUGUI CounterTime;
    [SerializeField] TextMeshProUGUI BestTitle;
    [SerializeField] TextMeshProUGUI BestTime;

    //timer
    private float timer;
    private float start_score;
    public bool counter_active;

    private void Start()
    {
        //disable canvas initially
        quest_canvas.gameObject.SetActive(false);
        timer = 0;
        counter_active = false;

        //add the unfinished quests to start
        foreach(Quest q in quests)
        {
            q.completed = false;
        }
    }

    private void Update()
    {
        //update the timer when this is active
        if(counter_active)
        {
            //update time if its time otherwise update score
            if(start_score == -1)
            {
                timer += Time.deltaTime;
                CounterTime.text = timer.ToString("F2");
            }
            else
            {
                CounterTime.text = (GameManager.instance.current_score - start_score).ToString();
            }
        }
    }

    //function to set this quest as the active quest
    public void SetActiveQuest(Quest q)
    {
        if(!active_quests.Contains(q) && !counter_active)
        {
            active_quests.Add(q);

            //update the canvas if necessary
            if (!q.find)
            {
                quest_canvas.gameObject.SetActive(true);

                //if its not a find quest (no necessary canvas)
                
                //if its a time trial
                if (q.time)
                {
                    //set the Title
                    CounterTitle.text = "CURRENT TIME";
                    timer = 0;

                    //set the best Info
                    BestTitle.text = "BEST TIME";
                    BestTime.text = q.best_time.ToString("F2");

                    //to make sure they differentiate
                    start_score = -1;
                }

                //if its a score trial
                if(q.trick)
                {
                    //set the Title
                    CounterTitle.text = "CURRENT SCORE";
                    start_score = GameManager.instance.current_score;

                    //set the best Info
                    BestTitle.text = "BEST SCORE";
                    BestTime.text = q.best_score.ToString();
                }

                //no matter what enable the timer
                counter_active = true;
            }
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

        //if its a find quest you auto complete
        if(q.find)
        {
            q.completed = true;
        }

        //handle the timer if its necessary
        if(counter_active)
        {
            //turn off the timer
            counter_active = false;

            //update the best time if necessary
            if(timer < q.best_time && q.time)
            {
                q.best_time = timer;
                q.completed = true;
            }

            //update best score if necessary
            if((GameManager.instance.current_score - start_score) > q.best_score && q.trick)
            {
                q.best_score = (GameManager.instance.current_score - start_score);
                q.completed = true;
            }

            //set both to 0 again
            timer = 0;
            start_score = 0;

            //do a quick pause before shutting down canvas
            StartCoroutine(delayCanvasClose());
        }
    }

    //function used to turn off canvas
    private IEnumerator delayCanvasClose()
    {
        yield return new WaitForSeconds(3f);

        quest_canvas.gameObject.SetActive(false);
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
        if (objectHolders[q].GetComponent<QuestObject>().delay_disappear)
        {
            StartCoroutine(DelayDisappear(objectHolders[q]));
        }
        else
        {
            objectHolders[q].SetActive(false);
        }
    }

    private IEnumerator DelayDisappear(GameObject go)
    {
        yield return new WaitForSeconds(10f);
        go.SetActive(false);
    }

}
