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

public class TestLobbies : BaseSingleton<TestLobbies>
{
    private Lobby _hostLobby, _joinedLobby;
    float _heartBeatTimer;
    float _lobbyUpdateTimer;
    [SerializeField] TextMeshProUGUI _txtCode;

    private async void Start()
    {
        await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };


        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //_txtCode = GetComponent<TextMeshProUGUI>();
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

    private async void CreateLobby()
    {
        string lobbyName = "Kaoru Mitoma";
        int maxPlayers = 4;

        //Tạo đc phòng thì dùng LobbyCode để join phòng
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
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void CreateALobby()
    {
        CreateLobby();
    }

    public void ListLobby()
    {
        ListLobbies();
    }

    public void JoinALobby()
    {
        string lobbyCode = _txtCode.text.Trim();
        Debug.Log("join success: " + lobbyCode);
        JoinLobbyByCode(lobbyCode);
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

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (var result in queryResponse.Results)
                Debug.Log("Lobby: " + result.Name + ", " + result.MaxPlayers);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            string cleanedLobbyCode = lobbyCode.Trim().Replace("\u200B", "");
            await Lobbies.Instance.JoinLobbyByCodeAsync(cleanedLobbyCode);

            Debug.Log("join success: " + cleanedLobbyCode);
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
