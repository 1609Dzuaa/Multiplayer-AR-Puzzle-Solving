using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static GameEnums;

public class LeaderboardController : PopupController
{
    [SerializeField] ItemLeaderboard _itemPrefab;
    [SerializeField] Transform _content;

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnRefreshLeaderboard, RefreshLeaderboard);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnRefreshLeaderboard, RefreshLeaderboard);
    }

    public void ButtonCloseClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupLeaderboard, false);
    }

    private void RefreshLeaderboard(object obj)
    {
        PlayerData[] arrPlayers = (PlayerData[])obj;
        foreach (Transform t in _content)
            Destroy(t.gameObject);

        List<PlayerData> tempList = new List<PlayerData>();
        foreach (var p in arrPlayers)
            tempList.Add(p);

        tempList = tempList.OrderByDescending(x => x.Score).ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            ItemLeaderboard item = Instantiate(_itemPrefab, _content);
            int order = i + 1;
            item.SetData(order, tempList[i]);
        }
    }
}
