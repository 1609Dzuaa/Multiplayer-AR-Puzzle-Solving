using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    string _prevLobbyId;

    #region Init & Destroy

    protected async override void Awake()
    {
        base.Awake();
        _listPlayers = new NetworkList<PlayerData>();
        _listPlayers.OnListChanged += ListPlayersOnChanged;
        EventsManager.Subscribe(EventID.OnUpdatePlayerData, UpdatePlayerData);
        EventsManager.Subscribe(EventID.OnNotifyWinner2, NotifyWinner);
        EventsManager.Subscribe(EventID.OnStakeDecrease, StakeDecreaseScore);
        EventsManager.Subscribe(EventID.OnEndMatch, HandleEndMatch);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //DontDestroyOnLoad(gameObject);
        //RelayManager.Instance.CreateRelay();
    }

    public override void OnDestroy()
    {
        //DontDestroyOnLoad(gameObject);
        base.OnDestroy();
        _listPlayers.OnListChanged -= ListPlayersOnChanged;
        EventsManager.Unsubscribe(EventID.OnUpdatePlayerData, UpdatePlayerData);
        EventsManager.Unsubscribe(EventID.OnNotifyWinner2, NotifyWinner);
        EventsManager.Unsubscribe(EventID.OnStakeDecrease, StakeDecreaseScore);
        EventsManager.Unsubscribe(EventID.OnEndMatch, HandleEndMatch);
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
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleClientDisconnect(ulong obj)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("IsHOST. a client disconnected");
        }
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

        EventsManager.Notify(EventID.OnRefreshLeaderboard, tempArr);

        RefreshLeaderboardClientRpc(tempArr);
        //Debug.Log("List changed");
    }

    #region Event's Callbacks

    private void UpdatePlayerData(object obj)
    {
        PlayerData playerData = (PlayerData)obj;
        _pData = playerData;
        if (IsServer)
        {
            //Debug.Log("server update player");
            UpdatePlayer(playerData, _playerIndex);
        }
        else if (IsOwner)
        {
            //Debug.Log("client update player");
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

    private void StakeDecreaseScore(object obj)
    {
        _pData.Score -= PowerupManager.Instance.ScoreStakeDecrease;
        List<PlayerData> tempList = new();
        foreach (var player in _listPlayers)
            tempList.Add(player);
        int index = tempList.FindIndex(x => x.Name == _pData.Name);
        _listPlayers[index] = _pData;
        Debug.Log("player: " + _pData.Name + " get deducted " + PowerupManager.Instance.ScoreStakeDecrease);
    }

    #endregion

    #region Lobby's Hearbeat

    private void Update()
    {
        if (_hostLobby != null)
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

                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                }
                catch (LobbyServiceException ex)
                {
                    Debug.Log(ex);
                }
            }
        }
    }

    #endregion

    private void ToggleConfigRoomAgain()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
        UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, true);
    }

    private async void CreateLobby(string lobbyName, int maxPlayers, int numOfRounds, int timeLimit, int timePrep)
    {
        if (String.IsNullOrEmpty(lobbyName))
        {
            string content = "Lobby Name Is Empty";
            UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
            ShowNotification.Show(content, ToggleConfigRoomAgain);
        }
        else if (maxPlayers < DEFAULT_TOTAL_PLAYER_TO_PLAY)
        {
            string content = "Cannot create a room under 3 players!";
            UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
            ShowNotification.Show(content, ToggleConfigRoomAgain);
        }
        else if (maxPlayers > DEFAULT_MAX_PLAYER)
        {
            string content = "Cannot create a room over 5 players!";
            UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
            ShowNotification.Show(content, ToggleConfigRoomAgain);
        }
        else
        {
            UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
            UIManager.Instance.TogglePopup(EPopupID.PopupWaiting, true);
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
                RoundManager.Instance.NumOfRounds.Value =  numOfRounds;
                RoundManager.Instance.RoundTimer.Value = timeLimit;
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
        EventsManager.Notify(EventID.OnRefreshLeaderboard, arrPlayers);
    }

    [ClientRpc]
    private void ReturnMenuClientRpc()
    {
        EventsManager.Notify(EventID.OnReturnMenu);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerDataServerRpc(PlayerData data, ulong clientId)
    {
        //EventsManager.Notify(EventID.OnCanPlay, data);
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

    [ServerRpc]
    private void RemovePlayerServerRpc(PlayerData playerLeaved)
    {
        _listPlayers.Remove(playerLeaved);
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
        EventsManager.Notify(EventID.OnCanPlay, _pData);
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
        object[] objs = { param, _listPlayers.Count };
        EventsManager.Notify(EventID.OnPopupWinner, objs);
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
            _hostLobby = _joinedLobby = null;
            _listPlayers.Clear();
            _isRelayConnected = _startCount = false;
            _playerIndex = 0;
            _prevLobbyId = "";
            _heartBeatTimer = 0f;
        }
        else
        {
            changes.ApplyToLobby(_joinedLobby);

            //có thằng join
            if (changes.PlayerJoined.Changed)
            {
                _isRelayConnected = true;
                NotifyClientRpc();
                //Debug.Log("Join Relay when lobby changes: ");
            }
            Debug.Log("Lobby changed");
        }
    }

    [ClientRpc]
    private void NotifyClientRpc()
    {
        EventsManager.Notify(EventID.OnCheckGameplayState, _joinedLobby);
    }

    private void SwitchToMainScene(string content)
    {
        NotificationParam param = new NotificationParam(content, TweenSwitchScene);
        UIManager.Instance.HideAllCurrentPopups();
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
        EventsManager.Notify(EventID.OnReceiveNotiParam, param);
    }

    private void TweenSwitchScene()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
        object[] objs = new object[] { _joinedLobby, null };
        EventsManager.Notify(EventID.OnStartGame, objs);
    }

    private void TweenSwitchScene2(string lobbyId)
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupLobby, false);
        object[] objs = new object[] { _joinedLobby, _prevLobbyId == lobbyId && !String.IsNullOrEmpty(lobbyId) };
        EventsManager.Notify(EventID.OnStartGame, objs);
    }

    public void CreateALobby(string lobbyName, int maxPlayers, int numOfRounds, int timeLimit, int timePrep)
    {
        CreateLobby(lobbyName, maxPlayers, numOfRounds, timeLimit, timePrep);
    }

    public void JoinALobby(string lobbyID)
    {
        JoinLobbyByID(lobbyID);
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

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, options);
            _joinedLobby = lobby;
            RelayManager.Instance.JoinRelay(_joinedLobby.Data[KEY_RELAY_CODE].Value);

            TweenSwitchScene2(lobbyID);
            if (String.IsNullOrEmpty(_prevLobbyId))
                _prevLobbyId = lobbyID;
            //TweenSwitchScene();
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

            EventsManager.Notify(EventID.OnRefreshLobby, response.Results);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void LeaveALobby()
    {
        LeaveLobby();
    }

    //có thể chia leave ra 2 loại:
    //1. client leave -> remove client đó và client đó tự shutdown (done)
    //2. host leave -> remove tất cả client và host tự shutdown và delete lobby (khi end game)
    //3. host leave giữa game -> thực hiện host migration, game vẫn tiếp tục (để sau)
    private async void LeaveLobby()
    {
        try
        {
            if (IsHost)
            {
                Debug.Log("host migrating");
                //MigrateLobbyHost();
                if (IsOwner)
                    RemovePlayerServerRpc(_pData);
            }
            else if (IsOwner)
                RemovePlayerServerRpc(_pData);

            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            //NetworkManager.Singleton.Shutdown();
            _hostLobby = _joinedLobby = null;
            Debug.Log("client leave lobby success");
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void HandleEndMatch(object obj)
    {
        //lúc bấm return ở end match
        if (NetworkManager.Singleton.IsHost)
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(_joinedLobby.Id);
                _listPlayers.Clear();
                ReturnMenuClientRpc();
                NetworkManager.Singleton.Shutdown();
                //DontDestroyOnLoad(gameObject);
                Debug.Log("Host delete lobby & leave");
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
        else
            LeaveALobby();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCreateNameServerRpc(string playerName, string playerKey, ulong clientId)
    {
        foreach (var p in _joinedLobby.Players)
            Debug.Log("playerKEYPLAERNAME: " + p.Data[KEY_PLAYER_NAME].Value);

        Player player = _joinedLobby.Players.Find(x => x.Data[KEY_PLAYER_NAME].Value == playerName);

        if (player != null)
        {
            //false
            CreateNameClientRpc(clientId, false, playerName);
            Debug.Log("found a player with name " + playerName);
            return;
        }

        //success
        TryCreateNameClientRpc(playerName, playerKey, clientId);
        //CreateNameClientRpc(clientId, true);
    }

    [ClientRpc]
    private void CreateNameClientRpc(ulong clientId, bool isSuccess, string playerName)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (isSuccess)
            {
                UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, false);
                string content = "Waiting for other players...";
                ShowNotification.Show(content);
                _pData = new PlayerData(playerName, DEFAULT_SCORE);
                Debug.Log("client send data to sv, name: " + playerName);
                SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                string content = "The name is already exist in Lobby, choose another name!";
                NotificationParam param = new NotificationParam(content, ToggleEnterNameAgain);
                EventsManager.Notify(EventID.OnReceiveNotiParam, param);
                UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, false);
                UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
            }
        }
        else
            Debug.Log("client not match Id");
    }

    private void ToggleEnterNameAgain()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false);
        UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, true);
    }

    [ClientRpc]
    private void TryCreateNameClientRpc(string playerName, string keyName, ulong clientId)
    {
        if (keyName != AuthenticationService.Instance.PlayerId) 
            return;

        Debug.Log("TryCreateNameClientRpc");

        TryCreateName(playerName, keyName, clientId);
    }

    private async void TryCreateName(string playerName, string keyName, ulong clientId)
    {
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
                _pData = new PlayerData(playerName, DEFAULT_SCORE);

                //tạo đc tên thì bắn event cho chơi
                //kèm data người chơi này
                //EventsManager.Notify(EventID.OnCanPlay, pData);
                /*UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, false);
                string content = "Waiting for other players...";
                ShowNotification.Show(content);

                SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);*/
                _joinedLobby = updatedLobby;
                //CreateNameClientRpc(clientId, true, playerName);
                UIManager.Instance.TogglePopup(EPopupID.PopupEnterName, false);
                string content = "Waiting for other players...";
                ShowNotification.Show(content);
                _pData = new PlayerData(playerName, DEFAULT_SCORE);
                Debug.Log("client send data to sv, name: " + playerName);
                SendPlayerDataServerRpc(_pData, NetworkManager.Singleton.LocalClientId);

                foreach (var p in _joinedLobby.Players)
                    Debug.Log("Lobby " + _joinedLobby.Name + ", Player: " + p.Data[KEY_PLAYER_NAME].Value);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void CreateNameInLobby(string playerName)
    {
        string keyName = AuthenticationService.Instance.PlayerId;
        if (IsHost)
        {
            RequestCreateNameServerRpc(playerName, keyName, NetworkManager.Singleton.LocalClientId);
            Debug.Log("host create name");
        }
        else
        {
            Debug.Log("isowner client");
            RequestCreateNameServerRpc(playerName, keyName, NetworkManager.Singleton.LocalClientId);
        }
    }

    public void PurchaseItem(Powerup powerup)
    {
        Debug.Log("player, score: " + _pData.Name + ", " + _pData.Score);
        if (powerup.Price <= _pData.Score)
        {
            PowerupManager.Instance.HandlePurchasePowerup(powerup);
        }
        else
        {
            string content = "Not enough score to buy";
            NotificationParam param = new NotificationParam(content, () => { });
            UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
            EventsManager.Notify(EventID.OnReceiveNotiParam, param);
            ShowNotification.Show(content);
        }
    }
}
