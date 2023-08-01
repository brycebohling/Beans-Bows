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
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    [SerializeField] Button createRelayBtn;
    [SerializeField] Button joinRelayBtn;
    [SerializeField] TMP_InputField joinCodeInput;


    private void Awake() 
    {
        Instance = this;
    }

    private async void Start() 
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.ClearSessionToken();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Sign in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


        
        createRelayBtn.onClick.AddListener(() => CreateRelay());
        joinRelayBtn.onClick.AddListener(() => JoinRelay(joinCodeInput.text));
    }

    public async Task<string> CreateRelay() 
    {
        try 
        {
            int maxPlayers = 4;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

            Debug.Log("Join Code: " + joinCode);
            return joinCode;

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
        

    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
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

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
