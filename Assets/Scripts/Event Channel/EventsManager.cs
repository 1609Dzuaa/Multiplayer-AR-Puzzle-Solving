using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public static class EventsManager //: BaseSingleton<EventsManager>
{
    private static Dictionary<EventID, Action<object>> _dictEvents = new Dictionary<EventID, Action<object>>();


    /*protected static override void Awake()
    {
        //base.Awake();
        //_dictEvents.Add(EventID.OnStartGame, OnStartGame);
        //_dictEvents.Add(EventID.OnCheckGameplayState, OnCheckGameplayState);
    }*/

    public static void Subscribe(EventID eventID, Action<object> callback)
    {
        if (!_dictEvents.ContainsKey(eventID))
            _dictEvents[eventID] = callback;
        else
            _dictEvents[eventID] += callback;
    }

    public static void Unsubscribe(EventID eventID, Action<object> callback)
    {
        if (_dictEvents.ContainsKey(eventID))
            _dictEvents[eventID] -= callback;
    }

    public static void Notify(EventID eventID, object eventArgs = null)
    {
        _dictEvents[eventID]?.Invoke(eventArgs);
    }
}
