using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;

public class RewardController : HintController
{
    [SerializeField] TextMeshProUGUI _txtHeader;

    protected override void ButtonLeftClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, false);
    }
}
