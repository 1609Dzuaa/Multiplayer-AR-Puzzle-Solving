using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupEnterName : PopupController
{
    [SerializeField] TMP_InputField _inputField;

    public void OnConfirmClick()
    {
        string inputName = _inputField.text.Replace("\u200B", "");
        LobbyManager.Instance.CreateNameInLobby(inputName);
        _inputField.text = "";
    }
}
