using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static GameEnums;

public class LobbyController : NetworkBehaviour
{
    [SerializeField] ItemRoomController _itemRoomPrefab;
    [SerializeField] Transform _contentRoom; //nơi chứa các prefabRoom

    const int BUTTON_CREATE = 0;
    const int BUTTON_JOIN = 1;
    const int BUTTON_CONFIRM = 2;

    private void Awake()
    {
        EventsManager.Instance.Subcribe(EventID.OnRefreshLobby, RefreshLobbyList);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventsManager.Instance.Unsubcribe(EventID.OnRefreshLobby, RefreshLobbyList);
    }

    private void Update()
    {
#if UNITY_EDITOR
        //if (IsOwner)
            //if (Input.GetKeyDown(KeyCode.Space))
                //RefreshLobbyList();
#endif
    }

    public void OnClick()
    {
        TestLobbies.Instance.RefreshLobbies();
    }

    private void RefreshLobbyList(object obj)
    {
        List<Lobby> listLobby = (List<Lobby>)obj;
        foreach (Transform t in _contentRoom)
            Destroy(t.gameObject);

        foreach (Lobby room in listLobby)
        {
            ItemRoomController newRoom = Instantiate(_itemRoomPrefab, _contentRoom);
            newRoom.SetupRoom(room.Id, room.Name, room.AvailableSlots, room.MaxPlayers);
        }

        Debug.Log("Room changed");
    }
}
