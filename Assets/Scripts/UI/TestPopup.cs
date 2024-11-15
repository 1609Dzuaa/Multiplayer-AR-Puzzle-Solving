using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPopup : MonoBehaviour
{
    public void OnClick()
    {
        UIManager.Instance.TogglePopup(GameEnums.EPopupID.PopupHint, false);
    }
}
