using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static GameConst;
using static GameEnums;

public class LobbyManager : NetworkSingleton<LobbyManager>
{
    private Lobby _hostLobby, _joinedLobby;
    private PlayerData _playerData;
    float _heartBeatTimer;
    bool _isRelayConnected = false;
    private NetworkList<PlayerData> _listPlayers;

    protected async override void Awake()
    {
        base.Awake();

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _listPlayers = new NetworkList<PlayerData>();
        _listPlayers.OnListChanged += ListPlayersOnChanged;
        //RelayManager.Instance.CreateRelay();
    }

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            Debug.Log("Client has joined.");
        }
    }

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

    private void Update()
    {
        HandleLobbyHeartBeat();
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

    private async void CreateLobby(string lobbyName, int maxPlayers)
    {
        if (String.IsNullOrEmpty(lobbyName))
        {
            string content = "Lobby Name Is Empty";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        }
        else if (maxPlayers < DEFAULT_TOTAL_PLAYER_TO_PLAY)
        {
            string content = "Cannot create a room under 3 players!";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        }
        else if (maxPlayers > DEFAULT_MAX_PLAYER)
        {
            string content = "Cannot create a room over 5 players!";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
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

    [ServerRpc]
    private void SendPlayerDataServerRpc(PlayerData data)
    {
        //EventsManager.Instance.Notify(EventID.OnCanPlay, data);
        _listPlayers.Add(data);
        Debug.Log("add " +  data.Name);
    }

    #endregion

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            
        }
        else
        {
            changes.ApplyToLobby(_joinedLobby);
            EventsManager.Instance.Notify(EventID.OnCheckGameplayState, _joinedLobby);

            //có thằng join
            if (changes.PlayerJoined.Changed && !_isRelayConnected)
            {
                _isRelayConnected = true;
                //RelayManager.Instance.JoinRelay(_joinedLobby.Data[KEY_RELAY_CODE].Value);
                //Debug.Log("Join Relay when lobby changes: ");
            }
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

    public void CreateALobby(string lobbyName, int maxPlayers)
    {
        CreateLobby(lobbyName, maxPlayers);
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

                //tạo đc tên thì bắn event cho chơi
                EventsManager.Instance.Notify(EventID.OnCanPlay);

                //sau đó sẽ tuỳ vào client/host mà gửi data vừa tạo để add vào listPlayer phía Host
                _playerData = new PlayerData(playerName, DEFAULT_SCORE);

                if (IsServer)
                    _listPlayers.Add(_playerData);
                else if (IsClient && IsOwner)
                {
                    SendPlayerDataServerRpc(_playerData);
                    Debug.Log("client send data SvRpc");
                }
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
