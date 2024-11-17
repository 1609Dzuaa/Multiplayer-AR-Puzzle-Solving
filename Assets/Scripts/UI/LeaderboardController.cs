using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class LeaderboardController : PopupController
{
    public void ButtonCloseClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupLeaderboard, false);
    }
}
