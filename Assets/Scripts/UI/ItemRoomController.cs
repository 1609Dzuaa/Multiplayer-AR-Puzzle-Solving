using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static GameEnums;

public class ItemRoomController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _txtID;
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] TextMeshProUGUI _txtTotalPlayer;
    Lobby _lobby;

    public void SetupRoom(Lobby lobby)
    {
        _lobby = lobby;
        _txtID.text = lobby.Id;
        _txtName.text = lobby.Name;
        _txtTotalPlayer.text = lobby.Players.Count + "/" + lobby.MaxPlayers.ToString();
    }

    public void ButtonJoinOnClick()
    {
        LobbyManager.Instance.JoinALobby(_lobby.Id);
        //UIManager.Instance.TogglePopup(EPopupID.PopupEnterCode, true);
        //EventsManager.Notify(EventID.OnRefreshLobby, _lobby);
    }
}
