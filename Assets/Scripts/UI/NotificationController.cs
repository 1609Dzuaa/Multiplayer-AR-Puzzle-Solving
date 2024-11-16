using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public struct NotificationParam
{
    public string Content;

    public NotificationParam(string content)
    {
        Content = content;
    }
}

public class NotificationController : HintController
{
    private void Awake()
    {
        EventsManager.Instance.Subcribe(EventID.OnReceiveNotiParam, ReceiveParam);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnReceiveNotiParam, ReceiveParam);
    }

    private void ReceiveParam(object obj)
    {
        NotificationParam param = (NotificationParam)obj;
        _txtHint.text = param.Content;
    }

    protected override void ButtonLeftClick()
    {
        
    }

    protected override void ButtonRightClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
    }
}
