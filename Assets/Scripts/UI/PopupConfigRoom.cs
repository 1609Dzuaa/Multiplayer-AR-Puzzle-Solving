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
    [SerializeField] TMP_Dropdown _dropdownRound, _dropdownTimeLimit, _dropdownPrepTime;

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

                int indexDropdown = _dropdownTimeLimit.value;
                string selectedOption = _dropdownTimeLimit.options[indexDropdown].text;
                int timeLimit = int.Parse(selectedOption.Substring(0, 2));

                int indexDropdown1 = _dropdownPrepTime.value;
                string selectedOption1 = _dropdownPrepTime.options[indexDropdown1].text;
                int timePrep = int.Parse(selectedOption1.Substring(0, 2));

                int indexDropdown2 = _dropdownRound.value;
                string selectedOption2 = _dropdownRound.options[indexDropdown2].text;
                int numOfRounds = int.Parse(selectedOption2.Substring(0, 1));
                Debug.Log("Val Rounds: " + numOfRounds);

                LobbyManager.Instance.CreateALobby(inputName, maxPlayers, numOfRounds, timeLimit, timePrep);
                break;

            case 2:
                LobbyManager.Instance.ListLobby();
                break;
        }
    }
}
