using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;
using static GameConst;
using Unity.Netcode;

public class RewardController : HintController
{
    [SerializeField] TextMeshProUGUI _txtHeader;

    private void Awake()
    {
        EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void Start()
    {
        //Debug.Log("do nothing");
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, ReceiveData);
    }

    private void ReceiveData(object obj)
    {
        Question questInfo = (Question)obj;

        int scoreReceived = questInfo.Score - QuestManager.Instance.ScoreDecrease * RoundManager.Instance.NumsOfObjTrackedCurrentRound.Value;
        if (PowerupManager.Instance.DoubleScore)
            scoreReceived *= DOUBLE;

        if (PowerupManager.Instance.Stake)
            scoreReceived += PowerupManager.Instance.ScoreStakeIncrease;

        //Debug.Log("isbomb: " + RoundManager.Instance.IsBombed.Value);
        if (RoundManager.Instance.IsBombed.Value)
        {
            scoreReceived = 0;
            if (RoundManager.Instance.IsHost)
                RoundManager.Instance.HandleBombServerRpc(false);
            else if (RoundManager.Instance.IsOwner)
            {
                Debug.Log("owner, defuse bomb");
                RoundManager.Instance.HandleBombServerRpc(false);
            }
        }
        _txtHint.text = "You Receive " + scoreReceived + " Points!";
    }

    protected override void ButtonLeftClick()
    {
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, false);
    }
}
