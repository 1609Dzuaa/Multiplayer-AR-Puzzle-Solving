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

    private const int BUTTON_NEXT_HINT = 0;
    private const int BUTTON_RETURN = 1;

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
        if (index == BUTTON_NEXT_HINT)
        {

        }
        else
        {

        }
    }
}
