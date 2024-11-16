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

        TweenPopupOn();
    }

    protected virtual void ResetComponent()
    {

    }

    protected void TweenPopupOn()
    {
        transform.DOScale(1.0f, _popupDuration).OnComplete(TweenChildComponent);
    }

    public void TweenPopupOff(TweenCallback callback)
    {
        transform.DOScale(0.0f, _popupDuration).OnComplete(callback.Invoke);
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
