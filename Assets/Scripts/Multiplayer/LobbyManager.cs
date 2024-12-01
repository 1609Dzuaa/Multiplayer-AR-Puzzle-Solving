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
using static GameEnums;

public class LobbyManager : BaseSingleton<LobbyManager>
{
    private Lobby _hostLobby, _joinedLobby;
    float _heartBeatTimer;
    float _lobbyUpdateTimer;

    const int DEFAULT_CURRENT_TOTAL_PLAYER = 1;
    const int DEFAULT_MAX_PLAYER = 5;
    const int DEFAULT_TOTAL_PLAYER_TO_PLAY = 3;

    private async void Start()
    {
        await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };


        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
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
            try
            {
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    //IsPrivate = true,

                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

                _hostLobby = lobby;
                _joinedLobby = _hostLobby;
                Debug.Log("lobby created: " + lobbyName + ", Players: " + maxPlayers + ", " + lobby.Id + ", " + lobby.LobbyCode);

                string content = "Create Lobby Success!";
                SwitchToMainScene(content);
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
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
        EventsManager.Instance.Notify(EventID.OnStartGame);
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

            string cleanedLobbylobbyID = lobbyID.Trim().Replace("\u200B", "");
            await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID);//JoinLobbyByIDAsync(cleanedLobbyCode);

            string content = "Join Lobby Success";
            SwitchToMainScene(content);
            Debug.Log("join success: " + cleanedLobbylobbyID);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;
            if (_lobbyUpdateTimer <= 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                _joinedLobby = lobby;
            }
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
}
