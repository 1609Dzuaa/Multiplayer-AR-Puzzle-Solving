using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class EventsManager : BaseSingleton<EventsManager>
{
    private Dictionary<EventID, Action<object>> _dictEvents = new Dictionary<EventID, Action<object>>();

    public void Subcribe(EventID eventID, Action<object> callback)
    {
        if (!_dictEvents.ContainsKey(eventID))
            _dictEvents.Add(eventID, callback);

        _dictEvents[eventID] += callback;
    }

    public void Unsubcribe(EventID eventID, Action<object> callback)
    {
        if (_dictEvents.ContainsKey(eventID))
            _dictEvents[eventID] -= callback;
    }

    public void Notify(EventID eventID, object eventArgs = null)
    {
        _dictEvents[eventID]?.Invoke(eventArgs);
    }
}
