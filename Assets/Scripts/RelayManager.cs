using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System;
using System.IO;

public class RelayManager : MonoBehaviour
{
    public UnityTransport _transport;
    public MainMenuUI _mainMenu;
    private async void Awake()
    {
        await Authenticate();
    }
    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    public async void CreateGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(4);
        _mainMenu._lobbyCode.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        NetworkManager.Singleton.StartHost();
    }
    public void StopGame()
    {
        NetworkManager.Singleton.Shutdown();
    }
    public async void JoinGame(string joinCode)
    {
        JoinAllocation a;
        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch(RelayServiceException)
        {
            Debug.Log("INVALID JOIN CODE");
            return;
        }
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort) a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
        return ;
    }
}
