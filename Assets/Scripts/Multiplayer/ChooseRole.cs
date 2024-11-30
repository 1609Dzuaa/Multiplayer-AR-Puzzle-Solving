using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChooseRole : MonoBehaviour
{
    const int BUTTON_SERVER = 0;
    const int BUTTON_CLIENT = 1;

    public void OnClick(int index)
    {
        switch(index)
        {
            case BUTTON_SERVER:
                NetworkManager.Singleton.StartServer();
                UIManager.Instance.StartMainMenu();
                break;

            case BUTTON_CLIENT:
                NetworkManager.Singleton.StartClient();
                UIManager.Instance.StartMainMenu();
                break;
        }
    }
}
