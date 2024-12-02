using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupEnterName : PopupController
{
    [SerializeField] TextMeshProUGUI _txtName;

    public void OnConfirmClick()
    {
        string inputName = _txtName.text.Replace("\u200B", "");
        LobbyManager.Instance.CreateNameInLobby(inputName);
    }
}
