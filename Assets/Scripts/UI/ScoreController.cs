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
        EventsManager.Subscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Subscribe(EventID.OnCanPlay, ReceivePlayerData);
        EventsManager.Subscribe(EventID.OnNotifyWinner1, SendDataToHost);
        EventsManager.Subscribe(EventID.OnPurchaseSuccess, SubtractScore);
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Unsubscribe(EventID.OnCanPlay, ReceivePlayerData);
        EventsManager.Unsubscribe(EventID.OnNotifyWinner1, SendDataToHost);
        EventsManager.Unsubscribe(EventID.OnPurchaseSuccess, SubtractScore);
    }

    private void SubtractScore(object obj)
    {
        int scoreSubtracted = (int)obj;
        DOTween.To(() => _score, x => _score = x, _score - scoreSubtracted, _duration).OnUpdate(
         () => _txtScore.text = "Score: " + _score.ToString()).OnComplete(
         () =>
         {
             _pData.Score = _score;
             //bắn event kêu host update data
             EventsManager.Notify(EventID.OnUpdatePlayerData, _pData);
             Debug.Log("done subtract score");
         });
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

        if (RoundManager.Instance.IsBombed.Value && !PowerupManager.Instance.Shield)
            scoreReceived = 0;
            
        DOTween.To(() => _score, x => _score = x, _score + scoreReceived, _duration).OnUpdate(
            () => _txtScore.text = "Score: " + _score.ToString()).OnComplete(
            () =>
            {
                _pData.Score = _score;
                //bắn event kêu host update data
                EventsManager.Notify(EventID.OnUpdatePlayerData, _pData);
                //Debug.Log("done tween score");
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
        EventsManager.Notify(EventID.OnNotifyWinner2, _pData);
    }
}
