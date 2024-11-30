using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static GameEnums;

[Serializable]
public struct Room : INetworkSerializable, IEquatable<Room>
{
    public FixedString32Bytes ID;
    public FixedString32Bytes Name;
    public int CurrentTotalPlayer;
    public int MaxPlayer;           

    public Room(FixedString32Bytes id, FixedString32Bytes name, int currentTotalPlayer, int maxPlayer)
    {
        ID = id;
        Name = name;
        CurrentTotalPlayer = currentTotalPlayer;
        MaxPlayer = maxPlayer;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref CurrentTotalPlayer);
        serializer.SerializeValue(ref MaxPlayer);
    }

    public bool Equals(Room other)
    {
        return ID == other.ID &&
               CurrentTotalPlayer == other.CurrentTotalPlayer &&
               MaxPlayer == other.MaxPlayer;
    }
}

public class RoomManager : NetworkSingleton<RoomManager>
{
    private NetworkList<Room> _listRoom;
    public NetworkList<Room> ListRoom { get { return _listRoom; } }
    const int DEFAULT_CURRENT_TOTAL_PLAYER = 1;
    const int DEFAULT_MAX_PLAYER = 5;
    const int DEFAULT_TOTAL_PLAYER_TO_PLAY = 2;

    protected override void Awake()
    {
        base.Awake();
        _listRoom = new NetworkList<Room>();

        Debug.Log("List Room created");
    }

    public void CreateRoom(FixedString32Bytes roomName, int maxPlayer = DEFAULT_MAX_PLAYER)
    {
        FixedString32Bytes idRoom = RoomIDGenerator.GenerateUniqueID();
        if (roomName.IsEmpty)
        {
            string content = "Room Name Is Empty";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        }
        else if (maxPlayer < DEFAULT_TOTAL_PLAYER_TO_PLAY)
        {
            string content = "Cannot create a room when the number of players is too low, a minimum of 3 players is required.";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        }
        else if (maxPlayer > DEFAULT_MAX_PLAYER)
        {
            string content = "Cannot create a room when the number of players is too high, maximum players allow is 5.";
            NotificationParam param = new NotificationParam(content);
            EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
        }
        else
        {
            NetworkManager.Singleton.StartHost();
            Room newRoom = new Room(idRoom, roomName, DEFAULT_CURRENT_TOTAL_PLAYER, maxPlayer);
            _listRoom.Add(newRoom);
            string content = "Create a room success!";
            NotificationParam param = new NotificationParam(content, TweenSwitchScene);
            UIManager.Instance.HideAllCurrentPopups();
            StartCoroutine(PopupInformation(param));
            Debug.Log("Start host success, room: " + idRoom + ", " + roomName);
        }
    }

    private IEnumerator PopupInformation(NotificationParam param)
    {
        yield return new WaitForSeconds(0.1f);

        UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
        EventsManager.Instance.Notify(EventID.OnReceiveNotiParam, param);
    }

    private void TweenSwitchScene()
    {
        EventsManager.Instance.Notify(EventID.OnStartGame);
    }

    public void JoinRoom()
    {

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_listRoom != null)
        {
            //ListRooms.Dispose();
        }
    }
}
