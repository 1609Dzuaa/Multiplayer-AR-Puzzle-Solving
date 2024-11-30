using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class ItemRoomController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _txtID;
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] TextMeshProUGUI _txtTotalPlayer;

    public void SetupRoom(FixedString32Bytes id, FixedString32Bytes name, int total, int maxPlayer)
    {
        _txtID.text = id.ToString();
        _txtName.text = name.ToString();
        _txtTotalPlayer.text = total.ToString() + "/" + maxPlayer.ToString();
    }

    public void ButtonJoinOnClick()
    {
        RoomManager.Instance.JoinRoom();
    }
}
