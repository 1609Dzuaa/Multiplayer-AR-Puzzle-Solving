using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static GameEnums;
using static GameConst;

public class RoundManager : NetworkSingleton<RoundManager>
{
    [SerializeField] TextMeshProUGUI _txtTimer, _txtRound;
    [SerializeField] float _delay;

    [HideInInspector] public NetworkVariable<int> CountRound = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> NumOfRounds = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> RoundTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> PrepTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> CountTime = new NetworkVariable<int>();

    [HideInInspector] public NetworkVariable<int> NumsOfObjTrackedCurrentRound = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<bool> IsBombed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Coroutine countdownCoroutine;
    private bool _isRestRound;

    protected override void Awake()
    {
        base.Awake();
        CountTime.OnValueChanged += OnCountTimeChanged;
        CountRound.OnValueChanged += OnCountRoundChanged;
        NumsOfObjTrackedCurrentRound.OnValueChanged += OnNumberTrackedChange;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CountTime.OnValueChanged -= OnCountTimeChanged;
        CountRound.OnValueChanged -= OnCountRoundChanged;
        NumsOfObjTrackedCurrentRound.OnValueChanged -= OnNumberTrackedChange;
    }

    private string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }

    private void OnCountRoundChanged(int previousValue, int newValue)
    {
        _txtRound.text = "Round " + newValue.ToString() + "/" + NumOfRounds.Value.ToString();
    }

    private void OnCountTimeChanged(int previousValue, int newValue)
    {
        _txtTimer.text = FormatTime(newValue);
    }

    private void OnNumberTrackedChange(int previousValue, int newValue)
    {
        if (IsServer)
        {
            if (newValue == DEFAULT_MAX_OBJECT_TRACKED)
            {
                if (CountRound.Value == NumOfRounds.Value)
                {
                    StopAllCoroutines();
                    StartCoroutine(DelayNotify1());
                    //Debug.Log("stop countdown, done match");
                    return;
                }

                _isRestRound = true;
                StopCoroutine(countdownCoroutine);
                StartCoroutine(CountdownCoroutine());
                //Debug.Log("max object round " + CountRound.Value + " reached!");
            }
        }
    }

    //delay notify đợi tween bên score xong nó gửi data lên sv đã r hẵng notify1
    private IEnumerator DelayNotify1()
    {
        yield return new WaitForSeconds(_delay);

        NotifyWinnerClientRpc();
    }

    #region RPCs

    [ServerRpc]
    public void StartRoundServerRpc()
    {
        CountTime.OnValueChanged += OnCountTimeChanged;
        CountRound.OnValueChanged += OnCountRoundChanged;
        StartCount();
    }

    [ServerRpc]
    public void IncreaseCountServerRpc()
    {
        NumsOfObjTrackedCurrentRound.Value++;
        //Debug.Log("Increase count: " + NumsOfObjTrackedCurrentRound.Value);
    }

    [ServerRpc]
    public void HandleBombServerRpc(bool isDefused)
    {
        IsBombed.Value = isDefused;
    }

    [ClientRpc]
    private void ResetNewRoundClientRpc(int currentRound = 1)
    {
        //NumsOfObjTrackedCurrentRound.Value = DEFAULT_MAX_OBJECT_TRACKED;
        EventsManager.Notify(EventID.OnReceiveQuest, currentRound);
        QuestManager.Instance.IsRestRound = false;
        _isRestRound = false;
        UIManager.Instance.ToggleButtonShop(false);
        string content = "Start round " + CountRound.Value + "!";
        ShowNotification.Show(content, () => UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false));
    }

    [ClientRpc]
    private void ResetPowerupClientRpc()
    {
        PowerupManager.Instance.ResetPowerups();
    }

    [ClientRpc]
    private void StartRestClientRpc()
    {
        _txtRound.text = "Rest Round";
        string content = "End of round " + CountRound.Value +", head to the Shop and buy some Power-ups";
        ShowNotification.Show(content, () => UIManager.Instance.TogglePopup(EPopupID.PopupInformation, false));
        UIManager.Instance.ToggleButtonShop(true);
        QuestManager.Instance.IsRestRound = true;
        if (PowerupManager.Instance.Stake && !PowerupManager.Instance.HintSolved)
        {
            Debug.Log("At rest round but hasn't finished solved question!");
            EventsManager.Notify(EventID.OnStakeDecrease);
        }
    }

    [ClientRpc]
    private void NotifyWinnerClientRpc()
    {
        EventsManager.Notify(EventID.OnNotifyWinner1);
    }

    #endregion

    private void StartCount()
    {
        CountRound.Value = FIRST_ROUND;
        CountTime.Value = RoundTimer.Value;
        //Debug.Log("Client started timer");

        _txtRound.text = "Round " + 1.ToString() + "/" + NumOfRounds.Value.ToString();
        _txtTimer.text = FormatTime(CountTime.Value);
        StartCountdown();
        ResetNewRoundClientRpc();
    }

    public void StartCountdown()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (CountRound.Value <= NumOfRounds.Value)
        {
            if (!_isRestRound)
                UIManager.Instance.ToggleButtonShop(false);

            while (CountTime.Value > 0 && !_isRestRound)
            {
                yield return new WaitForSeconds(1f);
                CountTime.Value -= 1;
                _txtTimer.text = FormatTime(CountTime.Value);
            }

            //found a winner
            if (CountRound.Value == NumOfRounds.Value && !_isRestRound)
            {
                NotifyWinnerClientRpc();
                yield break;
            }

            //rest round
            StartRestClientRpc();
            ResetPowerupClientRpc();
            CountTime.Value = PrepTimer.Value;

            while (CountTime.Value > 0)
            {
                yield return new WaitForSeconds(1);
                CountTime.Value -= 1;
            }

            CountRound.Value++;
            CountTime.Value = RoundTimer.Value;
            NumsOfObjTrackedCurrentRound.Value = 0;
            ResetNewRoundClientRpc(CountRound.Value);

            //Debug.Log("Finish a round: " + CountRound.Value + "/" + CountTime.Value);
        }

        //Debug.Log("Finish game");
    }
}
