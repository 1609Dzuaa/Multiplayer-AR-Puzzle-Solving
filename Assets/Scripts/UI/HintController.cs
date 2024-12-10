using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using static GameEnums;
using Unity.Netcode;

public class HintController : PopupController
{
    [SerializeField] protected float _tweenChildComponentDuration;
    [SerializeField] protected TextMeshProUGUI _txtHint;
    [SerializeField] protected Transform[] _arrBtns;

    protected const int BUTTON_LEFT_CLICK = 0;
    protected const int BUTTON_RIGHT_CLICK = 1;
    Question _currentQuest;

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnReceiveQuest, GetNextQuest);
        //EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, GetNextQuest);
        //_currentQuest = QuestManager.Instance.GetNextQuest();
        //_txtHint.text = (_currentQuest != null) ? _currentQuest.Hint : "No Hint Left";
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnReceiveQuest, GetNextQuest);
        //EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, GetNextQuest);
    }

    private void GetNextQuest(object obj = null)
    {
        //Question questRemove = obj as Question;
        NetworkVariable<int> currentRound = (NetworkVariable<int>)obj;
        _currentQuest = QuestManager.Instance.GetNextQuest(currentRound.Value);
        _txtHint.text = (_currentQuest != null) ? _currentQuest.Hint : "No Hint Left";
    }

    //override th này để nó đc xử lý trong callback OnComplete bên base
    protected override void TweenChildComponent()
    {
        base.TweenChildComponent();
        _txtHint.DOFade(1.0f, _tweenChildComponentDuration).OnComplete(() =>
        {
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < _arrBtns.Length; i++)
                sequence.Append(_arrBtns[i].DOScale(1.0f, _tweenChildComponentDuration));
        });
    }

    protected override void ResetComponent()
    {
        base.ResetComponent();
        for (int i = 0; i < _arrBtns.Length; i++)
            _arrBtns[i].localScale = Vector3.zero;
        _txtHint.DOFade(0f, .01f);
    }

    public void OnClick(int index)
    {
        if (index == BUTTON_LEFT_CLICK)
            ButtonLeftClick();
        else
            ButtonRightClick();
    }

    protected virtual void ButtonLeftClick()
    {
        GetNextQuest();

        //thiết kế extra-hint: thêm một btn nhỏ bên góc phải-trung tâm
        //bấm vào thì text sang hint kế, bấm lại thì sang hint trước
        /*string content = "Do You Want Next Hint ?";
        NotificationParam param = new NotificationParam(content);
        EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);*/
    }

    protected virtual void ButtonRightClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupHint, false);
    }
}
