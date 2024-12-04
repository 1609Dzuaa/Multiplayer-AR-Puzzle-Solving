using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static GameEnums;
using static GameConst;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] int _targetFrameRate;
    //private NetworkList<PlayerData> _listPlayers;
    

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = _targetFrameRate;
        //EventsManager.Instance.Subscribe(EventID.OnCanPlay, CreateListPlayers);
        //_listPlayers = new NetworkList<PlayerData>();
        //_listPlayers.OnListChanged += OnListPlayersChanged; 
    }

    /*private void CreateListPlayers(object obj)
    {
        if (obj != null)
        {
            if (IsHost)
            {
                PlayerData newPlayer = (PlayerData)obj;
                if (_listPlayers != null)
                    _listPlayers.Add(newPlayer);
                else
                    Debug.Log("List null cmnr");
                Debug.Log("add player " + newPlayer.Name + " into list in GameManager");
            }
        }

        if (IsClient)
            if (_listPlayers.Count == 0) Debug.Log("client list = 0");
            else
                foreach (var player in _listPlayers)
                    Debug.Log("Client saw player: " + player.Name);
    }*/

    public override void OnDestroy()
    {
        base.OnDestroy();
        //EventsManager.Instance.Unsubscribe(EventID.OnCanPlay, CreateListPlayers);
        //_listPlayers.OnListChanged -= OnListPlayersChanged;
    }

    private void OnListPlayersChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        //EventsManager.Instance.Notify(EventID.OnRefreshLeaderboard, _listPlayers);
    }
}