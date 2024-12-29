using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;
using static GameConst;
using DG.Tweening;

public class ScoreController : MonoBehaviour
{
    [SerializeField] float _duration;

    TextMeshProUGUI _txtScore;
    int _score = 0;
    PlayerData _pData;

    private void Awake()
    {
        _txtScore = GetComponent<TextMeshProUGUI>();
        EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Instance.Subscribe(EventID.OnCanPlay, ReceivePlayerData);
        EventsManager.Instance.Subscribe(EventID.OnNotifyWinner1, SendDataToHost);
        Debug.Log("Score sub");
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Instance.Unsubscribe(EventID.OnCanPlay, ReceivePlayerData);
        EventsManager.Instance.Unsubscribe(EventID.OnNotifyWinner1, SendDataToHost);
    }

    private void AddScore(object obj)
    {
        Question questInfo = (Question)obj;
        int scoreLeftFromObject = QuestManager.Instance.ScoreDecrease * RoundManager.Instance.NumsOfObjTrackedCurrentRound.Value;
        int scoreReceived = questInfo.Score - scoreLeftFromObject;

        if (PowerupManager.Instance.DoubleScore)
            scoreReceived *= DOUBLE;

        if (PowerupManager.Instance.Stake)
            scoreReceived += PowerupManager.Instance.ScoreStakeIncrease;

        DOTween.To(() => _score, x => _score = x, _score + scoreReceived, _duration).OnUpdate(
            () => _txtScore.text = "Score: " + _score.ToString()).OnComplete(
            () =>
            {
                _pData.Score = _score;
                //bắn event kêu host update data
                EventsManager.Instance.Notify(EventID.OnUpdatePlayerData, _pData);
                Debug.Log("done tween score");
            });
    }

    //bắn từ host cho phép chơi và cache data của bản thân player này tại đây
    private void ReceivePlayerData(object obj)
    {
        _pData = (PlayerData)obj;
        //Debug.Log("Can Play: " + _pData.Name);
    }

    private void SendDataToHost(object obj)
    {
        Debug.Log("send data to host");
        EventsManager.Instance.Notify(EventID.OnNotifyWinner2, _pData);
    }
}
