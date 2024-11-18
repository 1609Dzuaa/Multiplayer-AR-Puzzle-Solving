using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class SettingController : PopupController
{
    public void ButtonCloseClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupSetting, false);
    }
}
