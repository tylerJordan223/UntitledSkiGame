using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] Quest myQuest;

    public bool end_area;
    public bool storage_area;
    public bool delay_disappear;

    private void Start()
    {

        Debug.Log(QuestSystem.instance);

        if (storage_area)
        {
            QuestSystem.instance.addObjectHolder(myQuest, this.gameObject);
        }
        if(end_area)
        {
            QuestSystem.instance.addEndArea(myQuest, this.gameObject);
        }

        //disable the object it should not be on right now
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(end_area)
        {
            //this means that this is the area that ends the quest
            myQuest.EndQuest();
        }
    }
}
