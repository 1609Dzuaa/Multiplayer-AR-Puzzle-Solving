using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static GameConst;
using static GameEnums;

public class LobbyManager : BaseSingleton<LobbyManager>
{
    private Lobby _hostLobby, _joinedLobby;
    float _heartBeatTimer;
    bool _isRelayConnected = false;

    const int DEFAULT_CURRENT_TOTAL_PLAYER = 1;
    const int DEFAULT_MAX_PLAYER = 5;
    const string KEY_RELAY_CODE = "RelayCode";
    const string KEY_PLAYER_NAME = "PlayerName";

    protected async override void Awake()
    {
        base.Awake();

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //RelayManager.Instance.CreateRelay();
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

                    //Debug.Log("relay: " + _joinedLobby.Data[KEY_RELAY_CODE].Value);
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

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            
        }
        else
        {
            changes.ApplyToLobby(_joinedLobby);
            //Debug.Log("OnLobbyChanged");
            EventsManager.Instance.Notify(EventID.OnCheckGameplayState, _joinedLobby);

            //có thằng join
            if (changes.PlayerJoined.Changed && !_isRelayConnected)
            {
                _isRelayConnected = true;
                RelayManager.Instance.JoinRelay(_joinedLobby.Data[KEY_RELAY_CODE].Value);
                //Debug.Log("Join Relay Success: " + _joinedLobby.Data[KEY_RELAY_CODE].Value);
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
            //SetPlayerProfile("DefaultName");

            TweenSwitchScene();
            //string content = "Join Lobby Success";
            //SwitchToMainScene(content);
            //EventsManager.Instance.Notify(EventID.OnCheckGameplayState, _joinedLobby);
            //Debug.Log("join success: " + cleanedLobbylobbyID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    /*private async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;
            if (_lobbyUpdateTimer <= 0f)
            {
                float lobbyUpdateTimerMax = 1.5f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                _joinedLobby = lobby;
                EventsManager.Instance.Notify(EventID.OnCheckGameplayState, _joinedLobby);
            }
        }
    }*/

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

                EventsManager.Instance.Notify(EventID.OnCanPlay);
                _joinedLobby = updatedLobby;

                foreach (var p in _joinedLobby.Players)
                    Debug.Log("Lobby " + _joinedLobby.Name + ", Player: " + p.Data[KEY_PLAYER_NAME].Value);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

}
