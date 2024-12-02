using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class PopupLobby : PopupController
{
    [SerializeField] ItemRoomController _itemRoomPrefab;
    [SerializeField] Transform _contentRoom; //nơi chứa các prefabRoom

    const int BUTTON_CREATE = 0;
    const int BUTTON_REFRESH = 1;
    const int BUTTON_HOME = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClick(int index)
    {
        switch (index)
        {
            case BUTTON_CREATE:
                UIManager.Instance.TogglePopup(EPopupID.PopupConfigRoom, true);
                break;

            case BUTTON_REFRESH:
                //RoomManager.Instance.UpdateLobbyClientRpc(_contentRoom, _itemRoomPrefab);
                break;

            case BUTTON_HOME:
                UIManager.Instance.TogglePopup(EPopupID.PopupLobby, false);
                break;
        }
    }
}
