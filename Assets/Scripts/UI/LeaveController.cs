using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class LeaveController : PopupController
{ 
    public void ButtonCloseClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupLeaveGame, false);
    }
}
