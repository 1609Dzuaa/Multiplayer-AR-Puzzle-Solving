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
    [SerializeField] TMP_Dropdown _dropdown;

    public void OnClick(int index)
    {
        switch (index)
        {
            case BUTTON_BACK:
                UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, false);
                break;

            case BUTTON_CONFIRM:
                //xài tmp_input field gặp bug
                //https://discussions.unity.com/t/cannot-convert-inputfield-to-int/807904/12
                string inputName = _txtName.text;
                string totalPlayer = _txtTotalPlayer.text.Replace("\u200B", "");
                int maxPlayers;
                if (int.TryParse(totalPlayer.Trim(), out maxPlayers))
                    Debug.Log("Parse success");
                else
                    Debug.LogError("Invalid input: totalPlayer is not a valid number");

                int indexDropdown = _dropdown.value;
                string selectedOption = _dropdown.options[indexDropdown].text;
                Debug.Log("Val: " + selectedOption);

                LobbyManager.Instance.CreateALobby(inputName, maxPlayers);
                break;

            case 2:
                LobbyManager.Instance.ListLobby();
                break;
        }
    }
}
