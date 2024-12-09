using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkSingleton<RoundManager>
{
    [SerializeField] TextMeshProUGUI _txtTimer;
    [SerializeField] float _delayCountdown;
    [HideInInspector] public NetworkVariable<int> RoundTimer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> Count = new NetworkVariable<int>();
    NetworkVariable<float> _entryTime = new NetworkVariable<float>();
    private Coroutine countdownCoroutine;

    private void OnEnable()
    {
        Count.OnValueChanged += OnCountChanged;
    }

    private void OnDisable()
    {
        Count.OnValueChanged -= OnCountChanged;
    }

    private void OnCountChanged(int previousValue, int newValue)
    {
        _txtTimer.text = newValue.ToString();
    }

    [ServerRpc]
    public void StartRoundServerRpc()
    {
        Count.OnValueChanged += OnCountChanged;
        StartCoroutine(DelayStartCount());
    }

    [ClientRpc]
    public void NotifyCountdownStartClientRpc()
    {
        Debug.Log("Countdown started on client.");
        _txtTimer.text = Count.Value.ToString();
    }

    private IEnumerator DelayStartCount()
    {
        yield return new WaitForSeconds(_delayCountdown);

        _entryTime.Value = Time.time;
        Count.Value = 120;

        Debug.Log("Client started timer");

        _txtTimer.text = Count.Value.ToString();
        StartCountdown();
    }

    public void StartCountdown()
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (Count.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            Count.Value -= 1;
            _txtTimer.text = Count.Value.ToString();
            //NotifyCountdownStartClientRpc();
        }

        Debug.Log("Countdown finished!");
    }
}
