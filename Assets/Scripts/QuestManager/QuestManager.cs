using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : BaseSingleton<QuestManager>
{
    public List<Question> ListQuest;

    [Header("Tên ảnh của 1st quest")]
    [SerializeField] string _firstQuest;

    public Question GetRandomQuest(Question currentQuest = null)
    {
        return (currentQuest != null) ? ListQuest.Find(x => x != currentQuest) : ListQuest.Find(x => x.ImageName == _firstQuest);
    }

    public void RemoveQuest(Question questRemove)
    {
        ListQuest.Remove(questRemove);
    }
}
