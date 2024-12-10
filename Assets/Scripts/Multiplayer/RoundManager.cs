using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static GameEnums;

public class RoundManager : NetworkSingleton<RoundManager>
{
    [SerializeField] TextMeshProUGUI _txtTimer, _txtRound;
    [HideInInspector] public NetworkVariable<int> CountRound = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> NumOfRounds = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> RoundTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> PrepTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> CountTime = new NetworkVariable<int>();
    NetworkVariable<int> _round = new NetworkVariable<int>(1);
    NetworkVariable<float> _entryTime = new NetworkVariable<float>();
    private Coroutine countdownCoroutine;

    private void OnEnable()
    {
        CountTime.OnValueChanged += OnCountTimeChanged;
        CountRound.OnValueChanged += OnCountRoundChanged;
    }

    private void OnDisable()
    {
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
    private void GiveHintClientRpc()
    {
        EventsManager.Instance.Notify(EventID.OnReceiveQuest, CountRound);
    }

    #endregion

    private void StartCount()
    {
        _entryTime.Value = Time.time;
        CountRound.Value = 1;
        CountTime.Value = RoundTimer.Value;
        Debug.Log("Client started timer");

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

            CountRound.Value++;
            CountTime.Value = RoundTimer.Value;
            GiveHintClientRpc();
            //Debug.Log("Finish a round: " + CountRound.Value + "/" + CountTime.Value);
        }

        //Debug.Log("Finish game");
    }
}
