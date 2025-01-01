using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GameEnums;

public static class ShowNotification
{
    public static void Show(string content, UnityAction continueCallback = null)
    {
        NotificationParam param = new NotificationParam(content, continueCallback);
        EventsManager.Notify(EventID.OnReceiveNotiParam, param);
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
    }
}
