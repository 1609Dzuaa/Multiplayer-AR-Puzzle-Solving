using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupController : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    private Powerup powerup;

    public void PowerupOnClick()
    {
        LobbyManager.Instance.PurchaseItem(powerup);
    }

    public void Setup(Powerup powerup)
    {
        this.powerup = powerup;
        icon.sprite = powerup.PowerupIcon;
        price.text = powerup.Price.ToString();
        title.text = powerup.PowerupName;
        description.text = powerup.Description;
    }
}
