using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] Transform[] _arrBtn;
    [SerializeField] float _tweenDuration;

    private void OnEnable()
    {
        OnDisable();

        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < _arrBtn.Length; i++)
            sequence.Append(_arrBtn[i].DOScale(1.0f, _tweenDuration));
        //Debug.Log("Tween");
    }

    private void OnDisable()
    {
        for (int i = 0; i < _arrBtn.Length; i++)
            _arrBtn[i].transform.localScale = Vector3.zero;
    }
}
