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
    [SerializeField] Image _imgToggleBtn;
    bool _isForward = true;
    float _initialPosX;

    const int BUTTON_HINT = 0;
    const int BUTTON_LEADERBOARD = 1;
    const int BUTTON_SETTING = 2;
    const int BUTTON_OUT = 3;

    private void Start()
    {
        _initialPosX = transform.localPosition.x;
    }

    public void ToggleButtonOnClick()
    {
        //Debug.Log("localX: " + transform.localPosition.x);
        transform.DOLocalMoveX((_isForward) ? _initialPosX - _distance : _initialPosX, _tweenDuration);
        _imgToggleBtn.transform.localScale = (_isForward) ? new Vector3(-1, 1, 1) : Vector3.one;
        _isForward = !_isForward;
        Debug.Log("On Click");
    }

    public void IconOnClick(int index)
    {
        switch (index)
        {
            case BUTTON_HINT:
                UIManager.Instance.TogglePopup(EPopupID.PopupHint, true);
                ToggleButtonOnClick();
                break;

            case BUTTON_LEADERBOARD:
                UIManager.Instance.TogglePopup(EPopupID.PopupLeaderboard, true);
                ToggleButtonOnClick();
                break;

            case BUTTON_SETTING:
                break;

            case BUTTON_OUT:
                break;
        }
    }
}
