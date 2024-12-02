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
        OnRefreshLobby,
        OnCheckGameplayState, //lock/unlock gameplay dựa trên số ng
        OnCanPlay, //bắn sau khi đã đặt tên thành công

    }

    public enum EPopupID
    {
        PopupHint = 0,
        PopupLeaderboard = 1,
        PopupSetting = 2,
        PopupLeaveGame = 3,
        PopupInformation = 4,
        PopupReward = 5,
        PopupLobby = 6,
        PopupConfigRoom = 7,
        PopupEnterName = 8,

    }

    public enum ESoundName
    {
        BGM,
        Button1SFX,
        Button2SFX,
    }
}
