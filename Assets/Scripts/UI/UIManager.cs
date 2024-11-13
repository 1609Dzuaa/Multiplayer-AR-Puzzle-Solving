using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] MainMenuButton _mainMenuBtn;
    [SerializeField] Transform _mainMenuUI;
    [SerializeField] Transform _sceneTrans;
    [SerializeField] float _target;
    [SerializeField] float _duration;
    [SerializeField] Transform _blockObj;
    Vector3 _initPos = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        _mainMenuBtn.gameObject.SetActive(false);
        _initPos = _sceneTrans.position;
        EventsManager.Instance.Subcribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Subcribe(EventID.OnStartGame, StartGame);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Unsubcribe(EventID.OnStartGame, StartGame);
    }

    private void TweenButtons(object obj = null)
    {
        _mainMenuBtn.gameObject.SetActive(true);
    }

    private void StartGame(object obj = null)
    {
        float targetPos = _initPos.x + _target;

        _sceneTrans.DOLocalMoveX(targetPos, _duration).OnComplete(() =>
        {
            gameObject.SetActive(false);
            _blockObj.gameObject.SetActive(false);
            _sceneTrans.DOLocalMoveX(targetPos + _target, _duration).OnComplete(() => 
            {
                _sceneTrans.position = _initPos;
                EventsManager.Instance.Notify(EventID.OnPlay);
            });
        });
    }
}
