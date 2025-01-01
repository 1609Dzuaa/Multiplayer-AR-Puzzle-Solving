using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;

public class PopupWinner : PopupController
{
    [SerializeField] TextMeshProUGUI _txtWinner, _txtWinnerScore, _txtSelf, _txtSelfScore;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Awake()
    {
        EventsManager.Subscribe(EventID.OnPopupWinner, SetupTexts);
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe(EventID.OnPopupWinner, SetupTexts);
    }

    private void SetupTexts(object obj)
    {
        gameObject.SetActive(true);
        object[] data = (object[])obj;
        PlayerData[] pData = (PlayerData[])data[0];
        _txtWinner.text = pData[0].Name.ToString();
        _txtWinnerScore.text = pData[0].Score.ToString();
        _txtSelf.text = (pData[1].Rank + "/" + data[1]).ToString();
        _txtSelfScore.text = pData[1].Score.ToString();
        //Debug.Log("Winner: " + pData[0].Name);
        //Debug.Log("This Player Rank, Score: " + pData[1].Rank + ", " + pData[1].Score);
    }

    public void ReturnMenu()
    {
        EventsManager.Notify(EventID.OnEndMatch);
        //LobbyManager.Instance.LeaveALobby();
        //EventsManager.Notify(EventID.OnReturnMenu);
    }
}
