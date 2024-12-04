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
    [SerializeField] MainMenuButton _mainMenuBtn;
    [SerializeField] Transform _mainMenuUI;
    [SerializeField] Transform _sceneTrans;
    [SerializeField] Transform _dimmedBG;
    [SerializeField] Transform _mainContent;
    [SerializeField] float _distance;
    [SerializeField] float _duration;

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
        for (int i = 0; i < _arrARComponents.Length; i++)
            _arrARComponents[i].gameObject.SetActive(false);
        _initPos = _sceneTrans.localPosition;
        /*EventsManager.Instance.Subscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Subscribe(EventID.OnStartGame, StartGame);
        EventsManager.Instance.Subscribe(EventID.OnCheckGameplayState, CheckGameplayState);*/
        //Debug.Log($"Server IP: {NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].ClientId}");
    }

    private void Start()
    {
        EventsManager.Instance.Subscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Subscribe(EventID.OnStartGame, StartGame);
        EventsManager.Instance.Subscribe(EventID.OnCheckGameplayState, CheckGameplayState);
        EventsManager.Instance.Subscribe(EventID.OnCanPlay, AllowToPlay);

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
        EventsManager.Instance.Unsubscribe(EventID.OnLogoTweenCompleted, TweenButtons);
        EventsManager.Instance.Unsubscribe(EventID.OnStartGame, StartGame);
        EventsManager.Instance.Unsubscribe(EventID.OnCheckGameplayState, CheckGameplayState);
        EventsManager.Instance.Unsubscribe(EventID.OnCanPlay, AllowToPlay);
    }

    private void TweenButtons(object obj = null)
    {
        _mainMenuBtn.gameObject.SetActive(true);
    }

    private void StartGame(object obj)
    {
        Lobby joinedLobby = (Lobby)obj;

        float targetPos = _initPos.x + _distance;

        _sceneTrans.DOLocalMoveX(targetPos, _duration).OnComplete(() =>
        {

            for (int i = 0; i < _arrMainMenuComponents.Length; i++)
                _arrMainMenuComponents[i].gameObject.SetActive(false);

            HideAllCurrentPopups();

            _sceneTrans.DOLocalMoveX(targetPos + _distance, _duration).OnComplete(() =>
            {
                _sceneTrans.localPosition = _initPos;
                //thằng nào cũng phải truyền cái lobby nó join vào để check
                if (joinedLobby != null)
                    CheckGameplayState(joinedLobby);
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
                    TogglePopup(EPopupID.PopupInformation, false);
                if (!_canPlay)
                    TogglePopup(EPopupID.PopupEnterName, true);

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

        TogglePopup(EPopupID.PopupEnterName, false);
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
        EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
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
            _stackPopupOrder.Pop(); //bug client o day

            //if (_stackPopupOrder.Count > 0)
                //_stackPopupOrder.Peek().SetActive(true);
        }

        //Debug.Log("id: " + id);
        _dimmedBG.gameObject.SetActive((_stackPopupOrder.Count > 0) ? true : On);
    }

    public void HideAllCurrentPopups()
    {
        for (int i = 0; i <= _stackPopupOrder.Count; i++)
        {
            _stackPopupOrder.Pop().gameObject.SetActive(false);
        }
        _dimmedBG.gameObject.SetActive(false);
    }
}
