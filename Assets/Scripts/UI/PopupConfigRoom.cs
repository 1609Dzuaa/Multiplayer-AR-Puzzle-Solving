using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;

public class PopupConfigRoom : PopupController
{
    const int BUTTON_BACK = 0;
    const int BUTTON_CONFIRM = 1;

    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] TextMeshProUGUI _txtTotalPlayer;
    int _totalPlayer = 3;

    public void OnClick(int index)
    {
        switch (index)
        {
            case BUTTON_BACK:
                UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
                break;

            case BUTTON_CONFIRM:
                string inputName = _txtName.text;
                if (inputName.Length > 31)
                {
                    inputName = inputName.Substring(0, 31);
                    Debug.Log("Trim string: " +  inputName);
                }

                RoomManager.Instance.CreateRoom(inputName, _totalPlayer);
                break;
        }
    }
}
