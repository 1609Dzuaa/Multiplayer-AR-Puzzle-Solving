using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConst;

public class QuestManager : BaseSingleton<QuestManager>
{
    public List<Question> ListQuest;
    [HideInInspector] public int CurrentRound = FIRST_ROUND;
    [HideInInspector] public bool IsRestRound;

    public Question GetNextQuest(int questIndex)
    {
        //CurrentRound += 
        CurrentRound = questIndex + 1;
        Debug.Log("Get quest index: " + questIndex);
        return ListQuest[questIndex];
    }

    public void RemoveQuest()
    {
        //ListQuest.RemoveAt(INDEX_CURRENT_QUEST);
    }
}
