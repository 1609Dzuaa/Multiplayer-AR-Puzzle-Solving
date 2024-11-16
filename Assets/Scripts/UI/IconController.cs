using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using static GameEnums;

public class IconController : MonoBehaviour
{
    [SerializeField] float _tweenDuration;
    [SerializeField] float _distance;
    [SerializeField] Sprite[] _arrImgToggleBtn; //0 là in (trái), 1 là out (phải)
    [SerializeField] Image _imgToggleBtn;
    bool _isForward = true;
    float _initialPosX;

    const int BUTTON_HINT = 0;
    const int BUTTON_OUT = 1;
    const int BUTTON_SETTING = 2;

    private void Start()
    {
        _initialPosX = transform.localPosition.x;
    }

    public void ToggleButtonOnClick()
    {
        //Debug.Log("localX: " + transform.localPosition.x);
        transform.DOLocalMoveX((_isForward) ? _initialPosX - _distance : _initialPosX, _tweenDuration);
        _imgToggleBtn.sprite = (_isForward) ? _arrImgToggleBtn[1] : _arrImgToggleBtn[0];
        _isForward = !_isForward;
        //Debug.Log("On Click");
    }

    public void IconOnClick(int index)
    {
        switch (index)
        {
            case BUTTON_HINT:
                UIManager.Instance.TogglePopup(EPopupID.PopupHint, true);
                break;

            case BUTTON_OUT:

                break;

            case BUTTON_SETTING:
                break;
        }
    }
}
