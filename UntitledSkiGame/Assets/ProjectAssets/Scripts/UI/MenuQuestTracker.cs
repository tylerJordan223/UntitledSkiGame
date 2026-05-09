using UnityEngine;
using UnityEngine.UI;

public class MenuQuestTracker : MonoBehaviour
{
    //script used to handle the quest stars in the menu//

    [Header("QUEST")]
    [SerializeField] Quest my_quest;

    [Header("Star")]
    [SerializeField] Image star;

    private Color white = Color.white;
    private Color black = Color.black;

    private void Start()
    {
        if(my_quest.completed)
        {
            star.color = white;
        }
        else
        {
            star.color = black;
        }
    }

    private void Update()
    {
        if(my_quest.completed && star.color != white) 
        {
            star.color = white;
        }

        if(!my_quest.completed && star.color != black)
        {
            star.color = black;
        }
    }
}
