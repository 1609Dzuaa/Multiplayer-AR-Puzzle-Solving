using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class ShopController : PopupController
{
    [SerializeField] Transform parent;
    [SerializeField] PowerupController powerupPrefab;
    [SerializeField] Powerup[] arrayPowerup;

    public void Awake()
    {
        for (int i = 0; i < arrayPowerup.Length; i++)
        {
            PowerupController powerup = Instantiate(powerupPrefab, parent);
            powerup.Setup(arrayPowerup[i]);
        }
    }

    public void ButtonCloseClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupShop, false);
    }
}
