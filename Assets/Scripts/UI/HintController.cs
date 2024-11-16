using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class HintController : PopupController
{
    [SerializeField] float _tweenChildComponentDuration;
    [SerializeField] TextMeshProUGUI _txtHint;
    [SerializeField] Transform[] _arrBtns;

    private const int BUTTON_LEFT_CLICK = 0;
    private const int BUTTON_RIGHT_CLICK = 1;

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

    }

    protected virtual void ButtonRightClick()
    {
        UIManager.Instance.TogglePopup(GameEnums.EPopupID.PopupHint, false);
    }
}
