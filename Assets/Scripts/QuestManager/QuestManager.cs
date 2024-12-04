using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConst;

public class QuestManager : BaseSingleton<QuestManager>
{
    public List<Question> ListQuest;

    public Question GetNextQuest()
    {
        return ListQuest[INDEX_CURRENT_QUEST];
    }

    public void RemoveQuest()
    {
        ListQuest.RemoveAt(INDEX_CURRENT_QUEST);
    }
}
