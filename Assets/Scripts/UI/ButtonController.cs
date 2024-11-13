using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public enum ButtonIndex
{
    Start = 0,
    Tutorial = 1,
    Settings = 2,
    About = 3
}

public class ButtonController : MonoBehaviour
{
    bool _sceneLoaded = false;

    public void OnClick(int index)
    {
        switch (index) 
        {
            case (int)ButtonIndex.Start:
                if (!_sceneLoaded)
                {
                    _sceneLoaded = true;
                    EventsManager.Instance.Notify(EventID.OnStartGame);
                }
                break;

            case (int)ButtonIndex.Tutorial:

                break;

            case (int)ButtonIndex.Settings:

                break;

            case (int)ButtonIndex.About:

                break;
        }
    }
}
