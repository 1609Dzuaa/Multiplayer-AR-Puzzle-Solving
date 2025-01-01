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

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = _targetFrameRate;
        //_listPlayers = new NetworkList<PlayerData>();
        //_listPlayers.OnListChanged += OnListPlayersChanged; 
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        //EventsManager.Unsubscribe(EventID.OnCanPlay, CreateListPlayers);
        //_listPlayers.OnListChanged -= OnListPlayersChanged;
    }
}