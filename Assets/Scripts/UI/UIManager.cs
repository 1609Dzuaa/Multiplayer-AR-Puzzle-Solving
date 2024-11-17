using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using DG.Tweening;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct PopupStruct
{
    public EPopupID ID;
    public GameObject Popup;
}

public class UIManager : BaseSingleton<UIManager>
{
    [SerializeField] MainMenuButton _mainMenuBtn;
    [SerializeField] Transform _mainMenuUI;
    [SerializeField] Transform _sceneTrans;
    [SerializeField] float _distance;
    [SerializeField] float _duration;

    [Header("Các main component của AR system, mới vào thì giấu nó đi để tránh bug")]
    [SerializeField] Transform[] _arrARComponents; //đừng active component "UI" trước các component khác

    [Header("Popups")]
    [SerializeField] PopupStruct[] _arrPopups;
    Dictionary<EPopupID, GameObject> _dictPopups = new Dictionary<EPopupID, GameObject>();
    Vector3 _initPos = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        _mainMenuBtn.gameObject.SetActive(false);
        _initPos = _sceneTrans.localPosition;
        EventsManager.Instance.Subcribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Subcribe(EventID.OnStartGame, StartGame);
    }

    private void Start()
    {
        for (int i = 0; i < _arrPopups.Length; i++)
        {
            if (!_dictPopups.ContainsKey(_arrPopups[i].ID))
                _dictPopups.Add(_arrPopups[i].ID, _arrPopups[i].Popup);
            _arrPopups[i].Popup.SetActive(false);
        }
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
        float targetPos = _initPos.x + _distance;

        _sceneTrans.DOLocalMoveX(targetPos, _duration).OnComplete(() =>
        {
            gameObject.SetActive(false);

            for (int i = 0; i < _arrARComponents.Length; i++)
                _arrARComponents[i].gameObject.SetActive(true);

            _sceneTrans.DOLocalMoveX(targetPos + _distance, _duration).OnComplete(() =>
            {
                _sceneTrans.localPosition = _initPos;
            });
        });
    }

    public void TogglePopup(EPopupID id, bool On)
    {
        if (_dictPopups[id].gameObject.activeInHierarchy && On) return;

        //Maybe need to re-order render here
        if (On)
            _dictPopups[id].gameObject.SetActive(true);
        else
            _dictPopups[id].gameObject.GetComponent<PopupController>().TweenPopupOff(() => _dictPopups[id].gameObject.SetActive(false));
    }
}
