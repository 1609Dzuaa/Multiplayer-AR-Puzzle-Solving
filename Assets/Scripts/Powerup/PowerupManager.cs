using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using static GameConst;
using UnityEngine.UI;

public class PowerupManager : BaseSingleton<PowerupManager>
{
    [SerializeField] Image _shieldIcon;
    [SerializeField] Button _bombIcon;
    [HideInInspector] public bool DoubleScore = false;
    [HideInInspector] public bool Stake = false, HintSolved;
    [HideInInspector] public bool Shield = false;
    public int ScoreStakeIncrease, ScoreStakeDecrease;
    bool _bombed;

    protected override void Awake()
    {
        base.Awake();
        EventsManager.Subscribe(EventID.OnTrackedImageSuccess, AllowSetBomb);
    }

    private void AllowSetBomb(object obj) => _bombIcon.interactable = true;

    private void OnDestroy()
    {
        EventsManager.Unsubscribe(EventID.OnTrackedImageSuccess, AllowSetBomb);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void HandlePurchasePowerup(Powerup powerup)
    {
        EventsManager.Notify(EventID.OnPurchaseSuccess, powerup.Price);
        switch(powerup.PowerupName)
        {
            case DOUBLE_SCORE:
                DoubleScore = true;
                break;
            case STAKE:
                Stake = true;
                break;
            case BOMB:
                _bombIcon.gameObject.SetActive(true);
                gameObject.SetActive(true);
                break;
            case SHIELD:
                Shield = true;
                _shieldIcon.gameObject.SetActive(true);
                gameObject.SetActive(true);
                break;           
        }
        string content = "Purchase success!";
        ShowNotification.Show(content, () => UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false));
    }

    //sau 1 round thì reset powerup
    public void ResetPowerups()
    {
        DoubleScore = false;
        Stake = false;
        _bombIcon.gameObject.SetActive(false);
        _shieldIcon.gameObject.SetActive(false);
        gameObject.SetActive(false);
        _bombed = false;
        _bombIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        Shield = false;
    }

    public void SetBomb()
    {
        if (!_bombed)
        {
            _bombed = true;
            _bombIcon.GetComponent<Image>().color = new Color(0.45f, 0.45f, 0.45f, 0.45f);
            if (RoundManager.Instance.IsHost)
                RoundManager.Instance.HandleBombServerRpc(true);
            else if (RoundManager.Instance.IsOwner)
            {
                Debug.Log("owner, set bomb");
                RoundManager.Instance.HandleBombServerRpc(true);
            }

            string content = "Set bomb success!";
            NotificationParam param = new NotificationParam(content, () => UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false));
            UIManager.Instance.TogglePopup(EPopupID.PopupInformation, true);
            EventsManager.Notify(EventID.OnReceiveNotiParam, param);
        }
    }
}
