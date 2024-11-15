using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopupController : MonoBehaviour
{
    [SerializeField] protected float _popupDuration;
    [SerializeField] protected Ease _popupEase;
    protected bool _isFirstOnEnable = true;

    protected void OnEnable()
    {
        if (_isFirstOnEnable)
        {
            _isFirstOnEnable = false;
            ResetComponent();
            return;
        }

        TweenPopup();
    }

    protected virtual void ResetComponent()
    {

    }

    protected void TweenPopup()
    {
        transform.DOScale(1.0f, _popupDuration).OnComplete(TweenChildComponent);
    }

    protected virtual void TweenChildComponent()
    {
        Debug.Log("Tween child called");
    }

    protected void OnDisable()
    {
        transform.DOScale(0.0f, _popupDuration);
        ResetComponent();
    }
}
