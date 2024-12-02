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

    private void Start()
    {
        _txtScore = GetComponent<TextMeshProUGUI>();
        EventsManager.Instance.Subscribe(EventID.OnTrackedImageSuccess, AddScore);
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubscribe(EventID.OnTrackedImageSuccess, AddScore);
    }

    private void AddScore(object obj)
    {
        Question questInfo = (Question)obj;
        DOTween.To(() => _score, x => _score = x, _score + questInfo.Score, _duration).OnUpdate(() => _txtScore.text = "Score: " +_score.ToString());
    }

    private void Update()
    {
        _txtScore.text = (1.0f / Time.deltaTime).ToString();
    }
}
