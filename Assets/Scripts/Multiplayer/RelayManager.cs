using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using static GameConst;

public class RelayManager : BaseSingleton<RelayManager>
{
    public async Task<string> CreateRelay()
    {
        try
        {
            //await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));

            Allocation allocation = await Relay.Instance.CreateAllocationAsync(DEFAULT_MAX_PLAYERS_IN_LOBBY);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode + ", " + allocation.Region.ToString());

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            return "";
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
            StartCoroutine(DelayNoti());
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    private IEnumerator DelayNoti()
    {
        yield return new WaitForSeconds(1f);
        LobbyManager.Instance.TweenSwitchScene2();
    }
}
