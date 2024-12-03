using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemLeaderboard : MonoBehaviour
{
    [SerializeField] Sprite[] _arrSpriteBg;
    [SerializeField] Image _imageBg;
    [SerializeField] TextMeshProUGUI _txtOrder;
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] TextMeshProUGUI _txtScore;

    public void SetData(int order, PlayerData playerData)
    {
        _txtOrder.text = order.ToString();
        _txtName.text = playerData.Name.ToString();
        _txtScore.text = playerData.Score.ToString();
        _imageBg.sprite = _arrSpriteBg[order - 1];
    }
}
