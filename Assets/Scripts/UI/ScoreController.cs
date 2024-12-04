using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameEnums;
using DG.Tweening;

public class ScoreController : MonoBehaviour
{
    [SerializeField] float _duration;

    TextMeshProUGUI _txtScore;
    int _score = 0;
    PlayerData _pData;

    private void Start()
    {
        _txtScore = GetComponent<TextMeshProUGUI>();
        EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Instance.Subscribe(EventID.OnCanPlay, ReceivePlayerData);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, AddScore);
        EventsManager.Instance.Unsubscribe(EventID.OnCanPlay, ReceivePlayerData);
    }

    private void AddScore(object obj)
    {
        Question questInfo = (Question)obj;
        DOTween.To(() => _score, x => _score = x, _score + questInfo.Score, _duration).OnUpdate(
            () => _txtScore.text = "Score: " + _score.ToString()).OnComplete(
            () =>
            {
                _pData.Score = _score;
                EventsManager.Instance.Notify(EventID.OnUpdatePlayerData, _pData);
            });
    }

    private void ReceivePlayerData(object obj)
    {
        _pData = (PlayerData)obj;
    }

    /*private void Update()
    {
        _txtScore.text = (1.0f / Time.deltaTime).ToString();
    }*/
}
