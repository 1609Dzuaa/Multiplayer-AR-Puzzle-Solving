using DG.Tweening;
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
    [SerializeField] float _duration;

    const int BUTTON_CREATE = 0;
    const int BUTTON_JOIN = 1;
    const int BUTTON_CONFIRM = 2;
    bool _isFirstOnEnable = true;

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnRefreshLobby, RefreshLobbyList);
    }

    private void OnEnable()
    {
        if (_isFirstOnEnable)
        {
            _isFirstOnEnable = false;
            return;
        }
        LobbyManager.Instance.RefreshLobbies();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventsManager.Instance.Unsubscribe(EventID.OnRefreshLobby, RefreshLobbyList);
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
        LobbyManager.Instance.RefreshLobbies();
    }

    private void RefreshLobbyList(object obj)
    {
        List<Lobby> listLobby = (List<Lobby>)obj;
        foreach (Transform t in _contentRoom)
            Destroy(t.gameObject);

        Sequence sequence = DOTween.Sequence();

        foreach (Lobby room in listLobby)
        {
            ItemRoomController newRoom = Instantiate(_itemRoomPrefab, _contentRoom);
            newRoom.SetupRoom(room);
            newRoom.transform.localScale = Vector3.zero;
            sequence.Append(newRoom.transform.DOScale(1.0f, _duration));
        }

        Debug.Log("Room changed");
    }
}
