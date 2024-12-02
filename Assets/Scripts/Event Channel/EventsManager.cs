using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class EventsManager : BaseSingleton<EventsManager>
{
    private Dictionary<EventID, Action<object>> _dictEvents = new Dictionary<EventID, Action<object>>();

    private Action<object> OnStartGame;
    private Action<object> OnCheckGameplayState;

    protected override void Awake()
    {
        base.Awake();
        //_dictEvents.Add(EventID.OnStartGame, OnStartGame);
        //_dictEvents.Add(EventID.OnCheckGameplayState, OnCheckGameplayState);
    }

    public void Subscribe(EventID eventID, Action<object> callback)
    {
        if (!_dictEvents.ContainsKey(eventID))
            _dictEvents[eventID] = callback;
        else
            _dictEvents[eventID] += callback;
    }

    public void Unsubscribe(EventID eventID, Action<object> callback)
    {
        if (_dictEvents.ContainsKey(eventID))
            _dictEvents[eventID] -= callback;
    }

    public void Notify(EventID eventID, object eventArgs = null)
    {
        _dictEvents[eventID]?.Invoke(eventArgs);
    }
}
