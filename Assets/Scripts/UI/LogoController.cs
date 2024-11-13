using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using static GameEnums;

public class LogoController : MonoBehaviour
{
    [SerializeField] float _duration;
    [SerializeField] Ease _ease;
    Image _imgLogo;

    private void Awake()
    {
        _imgLogo = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _imgLogo.DOFade(0f, .01f);

        _imgLogo.DOFade(1.0f, _duration).OnComplete(() =>
        {
            EventsManager.Instance.Notify(EventID.OnLogoTweenCompleted);
        });
    }
}
