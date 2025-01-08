using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using static GameConst;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

[System.Serializable]
public struct PopupStruct
{
    public EPopupID ID;
    public GameObject Popup;
}

public class UIManager : BaseSingleton<UIManager>
{
    public MainMenuButton _mainMenuBtn;
    public GameObject Tutorial;
    public GameObject Setting;
    public GameObject About;
    [SerializeField] Transform _mainMenuUI;
    [SerializeField] Transform _sceneTrans;
    [SerializeField] Transform _dimmedBG;
    [SerializeField] Transform _mainContent;
    [SerializeField] float _distance;
    [SerializeField] float _duration;
    [SerializeField] GameObject _btnShop, _iconPowerups;

    [Header("Các main component của AR system, mới vào thì giấu nó đi để tránh bug")]
    [SerializeField] Transform[] _arrARComponents; //đừng active component "UI" trước các component khác

    [SerializeField] Transform[] _arrMainMenuComponents;

    [Header("Popups")]
    [SerializeField] PopupStruct[] _arrPopups;
    Dictionary<EPopupID, GameObject> _dictPopups = new Dictionary<EPopupID, GameObject>();
    Stack<GameObject> _stackPopupOrder = new Stack<GameObject>();
    Vector3 _initPos = Vector3.zero;
    bool _canPlay;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        /*for (int i = 0; i < _arrARComponents.Length; i++)
            _arrARComponents[i].gameObject.SetActive(false);*/
        _initPos = _sceneTrans.localPosition;
        UIManager.Instance.Tutorial.SetActive(false);
        UIManager.Instance.Setting.SetActive(false);
        UIManager.Instance.About.SetActive(false);
        _btnShop.SetActive(false);
        /*EventsManager.Subscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Subscribe(EventID.OnStartGame, StartGame);
        EventsManager.Subscribe(EventID.OnCheckGameplayState, CheckGameplayState);*/
        //Debug.Log($"Server IP: {NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].ClientId}");
    }

    private void Start()
    {
        EventsManager.Subscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Subscribe(EventID.OnStartGame, StartGame);
        EventsManager.Subscribe(EventID.OnCheckGameplayState, CheckGameplayState);
        EventsManager.Subscribe(EventID.OnCanPlay, AllowToPlay);
        EventsManager.Subscribe(EventID.OnReturnMenu, ReturnMenu);

        for (int i = 0; i < _arrARComponents.Length; i++)
            _arrARComponents[i].gameObject.SetActive(false);

        for (int i = 0; i < _arrPopups.Length; i++)
        {
            if (!_dictPopups.ContainsKey(_arrPopups[i].ID))
                _dictPopups.Add(_arrPopups[i].ID, _arrPopups[i].Popup);
            _arrPopups[i].Popup.SetActive(false);
        }

        _mainMenuBtn.gameObject.SetActive(false);
        //for (int i = 0; i < _arrARComponents.Length; i++)
        //_arrARComponents[i].gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Unsubscribe(EventID.OnStartGame, StartGame);
        EventsManager.Unsubscribe(EventID.OnCheckGameplayState, CheckGameplayState);
        EventsManager.Unsubscribe(EventID.OnCanPlay, AllowToPlay);
        EventsManager.Unsubscribe(EventID.OnReturnMenu, ReturnMenu);
    }

    private void TweenButtons(object obj = null)
    {
        _mainMenuBtn.gameObject.SetActive(true);
    }

    private void StartGame(object obj)
    {
        object[] objs = (object[])obj;
        Lobby joinedLobby = (Lobby)objs[0];
        bool isRejoin = false;
        if (objs[1] != null)
            isRejoin = (bool)objs[1];

        TweenSwitchScene(false, joinedLobby, isRejoin);
    }

    private void ReturnMenu(object obj)
    {
        _iconPowerups.SetActive(false);
        TogglePopup(EPopupID.PopupWinner, false);
        TweenSwitchScene(true);
    }

    private void TweenSwitchScene(bool backToMenu, Lobby joinedLobby = null, bool isRejoin = false)
    {
        float targetPos = _initPos.x + _distance;

        _sceneTrans.DOLocalMoveX(targetPos, _duration).OnComplete(() =>
        {
            //Debug.Log("back: " + backToMenu);
            for (int i = 0; i < _arrMainMenuComponents.Length; i++)
                _arrMainMenuComponents[i].gameObject.SetActive((!backToMenu) ? false : true);

            HideAllCurrentPopups();

            if (backToMenu)
            {
                for (int i = 0; i < _arrARComponents.Length; i++)
                    _arrARComponents[i].gameObject.SetActive(false);
                _dimmedBG.gameObject.SetActive(false);
                //SceneManager.LoadScene(0);
            }

            _sceneTrans.DOLocalMoveX(targetPos + _distance, _duration).OnComplete(() =>
            {
                _sceneTrans.localPosition = _initPos;
                /*if (!backToMenu)
                {
                    for (int i = 0; i < _arrARComponents.Length; i++)
                        _arrARComponents[i].gameObject.SetActive(true);
                }*/
                //thằng nào cũng phải truyền cái lobby nó join vào để check
                if (joinedLobby != null)
                {
                    if (!isRejoin)
                        CheckGameplayState(joinedLobby);
                    else
                        Debug.Log("rejoin");
                }
            });
        });
    }

    private void CheckGameplayState(object obj)
    {
        Lobby lobbyJoined = (Lobby)obj;

        //chỉ unlock khi đủ ng chơi
        if (lobbyJoined != null)
        {
            if (lobbyJoined.Players.Count >= DEFAULT_TOTAL_PLAYER_TO_PLAY)
            {
                if (_dictPopups[EPopupID.PopupInformation].activeInHierarchy)
                {
                    //Debug.Log("in4 false");
                    TogglePopup(EPopupID.PopupInformation, false);
                }
                if (!_canPlay)
                {
                    TogglePopup(EPopupID.PopupEnterName, true);
                    //Debug.Log("popup enter name");
                }

                //for (int i = 0; i < _arrARComponents.Length; i++)
                //_arrARComponents[i].gameObject.SetActive(true);

                //Debug.Log("stack: " + _stackPopupOrder.Count);
                //if (_stackPopupOrder.Count > 0)
                //TogglePopup(EPopupID.PopupInformation, false);
            }
            else
            {
                PopupLockGameplay(lobbyJoined);
            }
        }

        //Debug.Log("check state");
    }

    private void AllowToPlay(object obj)
    {
        for (int i = 0; i < _arrARComponents.Length; i++)
            _arrARComponents[i].gameObject.SetActive(true);

        TogglePopup(EPopupID.PopupInformation, false);
        _dimmedBG.gameObject.SetActive(false);
        _canPlay = true;
        //Debug.Log("stack: " + _stackPopupOrder.Count);
        //if (_stackPopupOrder.Count > 0)
            //TogglePopup(EPopupID.PopupInformation, false);
    }

    private void PopupLockGameplay(Lobby lobby)
    {
        string content = "Waiting for other players, current: " + lobby.Players.Count + "/" + lobby.MaxPlayers;
        NotificationParam param = new NotificationParam(content);
        TogglePopup(EPopupID.PopupInformation, true);
        EventsManager.Notify(EventID.OnReceiveNotiParam, param);
    }

    public void TogglePopup(EPopupID id, bool On)
    {
        //if (_dictPopups[id].activeInHierarchy && On) return;

        //Maybe need to re-order render here
        if (On)
        {
            //if (_stackPopupOrder.Count > 0)
                //_stackPopupOrder.Peek().GetComponent<PopupController>().TweenPopupOff(() => _stackPopupOrder.Peek().SetActive(false));

            _dictPopups[id].SetActive(true);
            _stackPopupOrder.Push(_dictPopups[id]);
        }
        else
        {
            _dictPopups[id].gameObject.GetComponent<PopupController>().TweenPopupOff(() => _dictPopups[id].SetActive(false));
            if (_stackPopupOrder.Count > 0)
                _stackPopupOrder.Pop(); //bug client o day

            //if (_stackPopupOrder.Count > 0)
                //_stackPopupOrder.Peek().SetActive(true);
        }

        //Debug.Log("id: " + id);
        _dimmedBG.gameObject.SetActive((_stackPopupOrder.Count > 0) ? true : On);
    }

    public void HideAllCurrentPopups()
    {
        for (int i = 0; i < _stackPopupOrder.Count; i++)
        {
            _stackPopupOrder.Pop().gameObject.SetActive(false);
        }
        _dimmedBG.gameObject.SetActive(false);
    }

    public void ToggleButtonShop(bool On) => _btnShop.SetActive(On);
}
