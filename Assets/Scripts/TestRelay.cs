using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.UI;
using TMPro;

public class TestRelay : MonoBehaviour
{
    [SerializeField] Button createRoomBtn;
    [SerializeField] Button joinRoomBtn;
    [SerializeField] TMP_InputField joinCodeInput;


    // private void Awake() 
    // {
    //     createRoomBtn.onClick.AddListener(() => 
    //     {
    //         CreateRelay();
    //     });

    //     joinRoomBtn.onClick.AddListener(() => 
    //     {
    //         JoinRelay(joinCodeInput.text);
    //     });
    // }

    // private async void Start()
    // {
    //     await UnityServices.InitializeAsync();

    //     AuthenticationService.Instance.SignedIn += () => {
    //         Debug.Log("Sign in " + AuthenticationService.Instance.PlayerId);
    //     };
    //     await AuthenticationService.Instance.SignInAnonymouslyAsync();
    // }

    
    // private void Update() {
    //     if (Input.GetKeyDown(KeyCode.C)) CreateRelay();
    // }

    private async void CreateRelay() 
    {
        try 
        {
            int maxPlayers = 4;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);
        
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string _joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " + _joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
