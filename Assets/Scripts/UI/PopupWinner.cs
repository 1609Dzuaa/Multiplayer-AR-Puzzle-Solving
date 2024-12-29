using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;

public class PopupWinner : PopupController
{
    [SerializeField] TextMeshProUGUI _txtWinner, _txtWinnerScore;

    protected override void OnEnable()
    {
        //base.OnEnable();
    }

    protected override void OnDisable()
    {
        //base.OnDisable();
    }

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnPopupWinner, SetupTexts);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnPopupWinner, SetupTexts);
    }

    private void SetupTexts(object obj)
    {
        PlayerData[] pData = (PlayerData[])obj;
        Debug.Log("Winner: " + pData[0].Name);
        Debug.Log("This Player Rank, Score: " + pData[1].Rank + ", " + pData[1].Score);
    }
}
