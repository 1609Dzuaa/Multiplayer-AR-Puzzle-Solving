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
        EventsManager.Instance.Subcribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void ReceiveData(object obj)
    {

    }

    protected override void ButtonLeftClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, false);
    }
}
