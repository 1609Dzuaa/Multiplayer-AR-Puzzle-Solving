using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainReturnButton : MonoBehaviour
{
    public void OnClick()
    {
        gameObject.SetActive(false);
        UIManager.Instance._mainMenuBtn.gameObject.SetActive(true);
    }
}
