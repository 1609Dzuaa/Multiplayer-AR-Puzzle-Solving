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
        OnReceiveQuestInfo,
        OnTrackedImageSuccess,

    }

    public enum EPopupID
    {
        PopupHint = 0,
        PopupLeaderboard = 1,
        PopupSetting = 2,
        PopupLeaveGame = 3,
        PopupInformation = 4,
        PopupReward = 5,
    }
}
