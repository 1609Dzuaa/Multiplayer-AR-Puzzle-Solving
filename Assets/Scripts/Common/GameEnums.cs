using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEnums
{
    public enum EventID
    {
        OnLogoTweenCompleted,
        OnStartGame,
        OnReceiveNotiParam,

    }

    public enum EPopupID
    {
        PopupHint = 0,
        PopupLeaderboard = 1,
        PopupInformation = 4,
    }
}
