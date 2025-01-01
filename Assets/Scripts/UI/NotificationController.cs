using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GameEnums;

public struct NotificationParam
{
    public string Content;
    public UnityAction ContinueCallback;
    public UnityAction YesCallback;
    public UnityAction NoCallback;

    public NotificationParam(string content, UnityAction continueCallback = null, UnityAction yesCallback = null, UnityAction noCallback = null)
    {
        Content = content;
        ContinueCallback = continueCallback;
        YesCallback = yesCallback;
        NoCallback = noCallback;
    }
}

public class NotificationController : HintController
{
    [SerializeField] Button _btnContinue;
    [SerializeField] Button _btnYes;
    [SerializeField] Button _btnNo;

    UnityAction[] _arrCallbacks;

    private void Awake()
    {
        EventsManager.Subscribe(EventID.OnReceiveNotiParam, ReceiveParam);
        _arrCallbacks = new UnityAction[3];
    }

    private void Start()
    {
        //do nothing
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe(EventID.OnReceiveNotiParam, ReceiveParam);
    }

    private void ReceiveParam(object obj)
    {
        NotificationParam param = (NotificationParam)obj;
        _txtHint.text = param.Content;
        _btnContinue.gameObject.SetActive(param.ContinueCallback != null);
        _btnYes.gameObject.SetActive(param.YesCallback != null);
        _btnNo.gameObject.SetActive(param.NoCallback != null);

        if (_arrCallbacks[0] != null)
        _btnContinue.onClick.RemoveListener(_arrCallbacks[0]);
        if (_arrCallbacks[1] != null)
            _btnYes.onClick.RemoveListener(_arrCallbacks[1]);
        if (_arrCallbacks[2] != null)
            _btnNo.onClick.RemoveListener(_arrCallbacks[2]);

        _arrCallbacks[0] = param.ContinueCallback;
        _arrCallbacks[1] = param.YesCallback;
        _arrCallbacks[2] = param.NoCallback;

        if (param.ContinueCallback != null)
            _btnContinue.onClick.AddListener(param.ContinueCallback);
        if (param.YesCallback != null)
            _btnYes.onClick.AddListener(param.YesCallback);
        if (param.NoCallback != null)
            _btnYes.onClick.AddListener(param.YesCallback);
    }

    public void OnClose()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
    }

    protected override void ButtonLeftClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
    }

    protected override void ButtonRightClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
    }
}
