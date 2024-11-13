using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseSingleton<GameManager>
{
    [SerializeField] int _targetFrameRate;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = _targetFrameRate;
    }
}
