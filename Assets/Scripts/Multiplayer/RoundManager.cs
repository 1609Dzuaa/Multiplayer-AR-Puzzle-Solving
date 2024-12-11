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
    [HideInInspector] public NetworkVariable<int> CountRound = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> NumOfRounds = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> RoundTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> PrepTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> CountTime = new NetworkVariable<int>();

    //NetworkVariable<int> _countRest
    NetworkVariable<int> _round = new NetworkVariable<int>(1);
    NetworkVariable<float> _entryTime = new NetworkVariable<float>();
    private Coroutine countdownCoroutine;

    protected override void Awake()
    {
        base.Awake();
        CountTime.OnValueChanged += OnCountTimeChanged;
        CountRound.OnValueChanged += OnCountRoundChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CountTime.OnValueChanged -= OnCountTimeChanged;
        CountRound.OnValueChanged -= OnCountRoundChanged;
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

    #region RPCs

    [ServerRpc]
    public void StartRoundServerRpc()
    {
        CountTime.OnValueChanged += OnCountTimeChanged;
        CountRound.OnValueChanged += OnCountRoundChanged;
        StartCount();
    }

    [ClientRpc]
    private void GiveHintClientRpc(int currentRound = 1)
    {
        EventsManager.Instance.Notify(EventID.OnReceiveQuest, currentRound);
    }

    [ClientRpc]
    private void StartRestClientRpc()
    {
        _txtRound.text = "Rest Round";
        string content = "End of round " + CountRound.Value +", head to the Shop and buy some Power-ups";
        ShowNotification.Show(content, () => { });
    }

    #endregion

    private void StartCount()
    {
        _entryTime.Value = Time.time;
        CountRound.Value = FIRST_ROUND;
        CountTime.Value = RoundTimer.Value;
        //Debug.Log("Client started timer");

        _txtRound.text = "Round " + 1.ToString() + "/" + NumOfRounds.Value.ToString();
        _txtTimer.text = FormatTime(CountTime.Value);
        StartCountdown();
        GiveHintClientRpc();
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
            while (CountTime.Value > 0)
            {
                yield return new WaitForSeconds(1f);
                CountTime.Value -= 1;
                _txtTimer.text = FormatTime(CountTime.Value);
            }

            //rest round
            StartRestClientRpc();
            CountTime.Value = PrepTimer.Value;

            while (CountTime.Value > 0)
            {
                yield return new WaitForSeconds(1);
                CountTime.Value -= 1;
            }

            CountRound.Value++;
            CountTime.Value = RoundTimer.Value;
            GiveHintClientRpc(CountRound.Value);
            //Debug.Log("Finish a round: " + CountRound.Value + "/" + CountTime.Value);
        }

        //Debug.Log("Finish game");
    }
}
