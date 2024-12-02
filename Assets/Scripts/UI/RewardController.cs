using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;

public class RewardController : HintController
{
    [SerializeField] TextMeshProUGUI _txtHeader;

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void Start()
    {
        //Debug.Log("do nothing");
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void ReceiveData(object obj)
    {
        Question questInfo = (Question)obj;
        _txtHint.text = "You Receive " + questInfo.Score + " Points!";
    }

    protected override void ButtonLeftClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, false);
    }
}
