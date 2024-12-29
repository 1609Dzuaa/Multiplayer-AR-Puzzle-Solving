using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static GameConst;
using static GameEnums;

public class LobbyManager : NetworkSingleton<LobbyManager>
{
    Lobby _hostLobby, _joinedLobby;
    //public PlayerData PlayerData;
    float _heartBeatTimer;
    bool _isRelayConnected = false, _startCount = false;
    NetworkList<PlayerData> _listPlayers;
    PlayerData _pData;
    int _playerIndex = 0;

    #region Init & Destroy

    protected async override void Awake()
    {
        base.Awake();
        _listPlayers = new NetworkList<PlayerData>();
        _listPlayers.OnListChanged += ListPlayersOnChanged;
        //_playerIndex.OnValueChanged += OnSomeValueChanged;
        EventsManager.Instance.Subscribe(EventID.OnUpdatePlayerData, UpdatePlayerData);
        EventsManager.Instance.Subscribe(EventID.OnNotifyWinner2, NotifyWinner);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //RelayManager.Instance.CreateRelay();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _listPlayers.OnListChanged -= ListPlayersOnChanged;
        EventsManager.Instance.Unsubscribe(EventID.OnUpdatePlayerData, UpdatePlayerData);
        EventsManager.Instance.Unsubscribe(EventID.OnNotifyWinner2, NotifyWinner);
    }

    #endregion

    #region Ownership
    /// <summary>
    /// Các đoạn mã này sẽ chuyển Ownership cho thằng client vừa kết nối
    /// bởi vì mặc định IsOwner trên client sẽ = false do có thằng host (2 thằng share chung scr)
    /// chuyển như này mới có thể sử dụng IsOwner và gọi Rpc được
    /// </summary>
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            //Khi client kết nối, thay đổi quyền sở hữu cho client đó
            Debug.Log("new client has connected, change owner to: " + clientId);
            GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }
    }
    #endregion

    private void ListPlayersOnChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        //chuyển đổi vì Netcode kh hỗ trợ serialization NetworkList
        PlayerData[] tempArr = new PlayerData[_listPlayers.Count];
        for(int i = 0; i < tempArr.Length; i++)
            tempArr[i] = _listPlayers[i];

        EventsManager.Instance.Notify(EventID.OnRefreshLeaderboard, tempArr);

        RefreshLeaderboardClientRpc(tempArr);
        Debug.Log("List changed");
    }

    #region Event's Callbacks

    private void UpdatePlayerData(object obj)
    {
        PlayerData playerData = (PlayerData)obj;
        if (IsServer)
        {
            Debug.Log("server update player");
            UpdatePlayer(playerData, _playerIndex);
        }
        else if (IsOwner)
        {
            Debug.Log("client update player");
            UpdatePlayerDataServerRpc(playerData, _playerIndex);
        }
    }

    private void NotifyWinner(object obj)
    {
        if (IsServer)
        {
            PlayerData thisPlayer = (PlayerData)obj;
            //do networkList 0 hỗ trợ Linq
            List<PlayerData> tempList = new();
            foreach (var player in _listPlayers)
                tempList.Add(player);

            tempList = tempList.OrderByDescending(x => x.Score).ToList();
            PopupWinnerClientRpc(tempList.ToArray());
        }
    }

    #endregion

    #region Lobby's Hearbeat

    private void Update()
    {
        HandleLobbyHeartBeat();
        /*if (_listPlayers != null)
            foreach (var p in _listPlayers)
                Debug.Log("name, scr: " + p.Name + ", " + p.Score);*/
        //HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (_hostLobby != null)
        {
            _heartBeatTimer -= Time.deltaTime;
            if (_heartBeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                _heartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    #endregion

    private async void CreateLobby(string lobbyName, int maxPlayers, int numOfRounds, int timeLimit, int timePrep)
    {
        if (String.IsNullOrEmpty(lobbyName))
        {
            string content = "Lobby Name Is Empty";
            ShowNotification.Show(content);
        }
        else if (maxPlayers < DEFAULT_TOTAL_PLAYER_TO_PLAY)
        {
            string content = "Cannot create a room under 3 players!";
            ShowNotification.Show(content);
        }
        else if (maxPlayers > DEFAULT_MAX_PLAYER)
        {
            string content = "Cannot create a room over 5 players!";
            ShowNotification.Show(content);
        }
        else
        {
            string defaultName = "UIT";
            try
            {
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, defaultName) }
                        }
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                string relayCode = await RelayManager.Instance.CreateRelay();

                _hostLobby = lobby;
                _joinedLobby = _hostLobby;
                _playerIndex = INDEX_OF_HOST;
                RoundManager.Instance.NumOfRounds.Value = 2;// numOfRounds;
                RoundManager.Instance.RoundTimer.Value = 20;
                RoundManager.Instance.PrepTimer.Value = timePrep;

                try
                {
                    Lobby updateLobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
                    {
                            Data = new Dictionary<string, DataObject>
                        {
                            { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
                    });

                    _joinedLobby = updateLobby;
                }
                catch(LobbyServiceException ex)
                {
                    Debug.LogException(ex);
                }

                //assign callbacks for upcoming Lobby updates
                var callback = new LobbyEventCallbacks();
                callback.LobbyChanged += OnLobbyChanged;
                try
                {
                    await Lobbies.Instance.SubscribeToLobbyEventsAsync(_joinedLobby.Id, callback);
                }
                catch (LobbyServiceException ex)
                {
                    switch (ex.Reason)
                    {
                        case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{_hostLobby.Name}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                        case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                        case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                        default: throw;
                    }
                }
                //Debug.Log("lobby created: " + lobbyName + ", Players: " + maxPlayers + ", " + lobby.Id + ", " + lobby.LobbyCode);

                string content = "Create Lobby Success!";
                SwitchToMainScene(content);
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
    }

    #region RPC's Calls

    [ClientRpc]
    private void RefreshLeaderboardClientRpc(PlayerData[] arrPlayers)
    {
        EventsManager.Instance.Notify(EventID.OnRefreshLeaderboard, arrPlayers);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerDataServerRpc(PlayerData data, ulong clientId)
    {
        //EventsManager.Instance.Notify(EventID.OnCanPlay, data);
        //playerIndex = _listPlayers.Count;
        UpdatePlayerIndexClientRpc(_listPlayers.Count, clientId);
        _listPlayers.Add(data);
        //Debug.Log("add " +  data.Name);
        if (_listPlayers.Count == DEFAULT_TOTAL_PLAYER_TO_PLAY && !_startCount)
        {
            _startCount = true;
            AllowToPlayClientRpc();
            RoundManager.Instance.StartRoundServerRpc();
        }
    }

    [ClientRpc]
    private void UpdatePlayerIndexClientRpc(int playerIndex, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
            _playerIndex = playerIndex;
    }

    [ClientRpc]
    private void AllowToPlayClientRpc()
    {
        EventsManager.Instance.Notify(EventID.OnCanPlay, _pData);
        //Debug.Log("Fire Can Play:" + _pData.Name);
    }

    [ServerRpc]
    private void UpdatePlayerDataServerRpc(PlayerData data, int playerIndex)
    {
        UpdatePlayer(data, playerIndex);
        //Debug.Log("Update Data");
    }

    [ClientRpc]
    private void PopupWinnerClientRpc(PlayerData[] arrOrdered)
    {
        int rank = arrOrdered.ToList().FindIndex(x => x.Name == _pData.Name) + PLUS_ONE_BECAUSE_IS_INDEX;
        _pData.Rank = rank;
        PlayerData winner = arrOrdered[0];
        if (IsServer)
        {
            winner.Rank = _pData.Rank;
            _pData = winner;
        }
        PlayerData[] param = { winner, _pData };
        EventsManager.Instance.Notify(EventID.OnPopupWinner, param);
    }

    #endregion

    private void UpdatePlayer(PlayerData data, int playerIndex)
    {
        //Debug.Log("player, index: " + data.Name + ", " + playerIndex);
        _listPlayers[playerIndex] = data;
        //foreach (var player in _listPlayers)
            //Debug.Log("name, score: " + player.Name + ", " + player.Score);
    }

    private void PopupWinner()
    {

    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            
        }
        else
        {
            changes.ApplyToLobby(_joinedLobby);

            //có thằng join
            if (changes.PlayerJoined.Changed && !_isRelayConnected)
            {
                _isRelayConnected = true;
                EventsManager.Instance.Notify(EventID.OnCheckGameplayState, _joinedLobby);
                //RelayManager.Instance.JoinRelay(_joinedLobby.Data[KEY_RELAY_CODE].Value);
                //Debug.Log("Join Relay when lobby changes: ");
            }
            //Debug.Log("Lobby changed");
        }
    }

    private void SwitchToMainScene(string content)
    {
        NotificationParam param = new NotificationParam(content, TweenSwitchScene);
        UIManager.Instance.HideAllCurrentPopups();
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
        EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
    }

    private void TweenSwitchScene()
    {
        EventsManager.Instance.Notify(EventID.OnStartGame, _joinedLobby);
    }

    public void CreateALobby(string lobbyName, int maxPlayers, int numOfRounds, int timeLimit, int timePrep)
    {
        CreateLobby(lobbyName, maxPlayers, numOfRounds, timeLimit, timePrep);
    }

    public void ListLobby()
    {
        ListLobbies();
    }

    public void JoinALobby(string lobbyID)
    {
        JoinLobbyByID(lobbyID);
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,
                    "0",
                    QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            //để tham số default thì lấy mọi lobby
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count + ", " + queryResponse.Results[0].Players.Count);
            foreach (var result in queryResponse.Results)
                Debug.Log("Lobby: " + result.Name + ", " + result.MaxPlayers);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void JoinLobbyByID(string lobbyID)
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            //string cleanedLobbylobbyID = lobbyID.Trim().Replace("\u200B", "");

            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                        {
                            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "UIT") }
                        }
                }
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, options);//JoinLobbyByIDAsync(cleanedLobbyCode);
            _joinedLobby = lobby;
            RelayManager.Instance.JoinRelay(_joinedLobby.Data[KEY_RELAY_CODE].Value);

            TweenSwitchScene();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void RefreshLobbies()
    {
        RefreshLobbyList();
    }

    private async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value:"0")
            };

            options.Order = new List<QueryOrder>
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();//options);

            EventsManager.Instance.Notify(EventID.OnRefreshLobby, response.Results);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void CreateNameInLobby(string playerName)
    {
        string keyName = AuthenticationService.Instance.PlayerId;

        Player player = _joinedLobby.Players.Find(x => x.Data[KEY_PLAYER_NAME].Value == playerName);

        if (player != null)
        {
            string content = "The name " + playerName + " is already exist in Lobby, choose another name!";
            NotificationParam param = new NotificationParam(content, () => { UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false); });
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
            return;
        }

        UpdatePlayerOptions playerOptions = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
        };

        try
        {
            Lobby updatedLobby = await Lobbies.Instance.UpdatePlayerAsync(_joinedLobby.Id, keyName, playerOptions);

            if (updatedLobby != null)
            {
                /*string successMessage = "Your name has been updated successfully!";
                NotificationParam successParam = new NotificationParam(successMessage, () => { UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false); });
                EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, successParam);*/

                _pData = new PlayerData(playerName, DEFAULT_SCORE);

                //tạo đc tên thì bắn event cho chơi
                //kèm data người chơi này
                //EventsManager.Instance.Notify(EventID.OnCanPlay, pData);
                UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, false);
                string content = "Waiting for other players...";
                ShowNotification.Show(content);

                SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);

                //dựa vào là host hay client để gửi data và đánh index cho player
                /*if (IsHost) //problem with owner
                {
                    SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);
                    Debug.Log("server send data SvRpc");
                }
                else if (IsClient && IsOwner)
                {
                    SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);
                    Debug.Log("client send data SvRpc");
                }*/
                _joinedLobby = updatedLobby;

                //foreach (var p in _joinedLobby.Players)
                    //Debug.Log("Lobby " + _joinedLobby.Name + ", Player: " + p.Data[KEY_PLAYER_NAME].Value);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

}
