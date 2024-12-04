using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonShopController : MonoBehaviour
{
    public void OnClick()
    {
        UIManager.Instance.TogglePopup(GameEnums.EPopupID.PopupShop, true);
    }
}
