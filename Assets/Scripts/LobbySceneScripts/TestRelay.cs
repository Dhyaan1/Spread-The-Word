using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;

public class TestRelay : MonoBehaviour
{

    // Start is called before the first frame update
    // private async void Start()
    // {
    //     await UnityServices.InitializeAsync();

    //     AuthenticationService.Instance.SignedIn += () =>
    //     {
    //         Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
    //     };

    //     await AuthenticationService.Instance.SignInAnonymouslyAsync();
    // }


    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Join Code: " + JoinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();


        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with joinCode: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("Allocation joined " + joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateRelay();
        }
        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     JoinRelay();
        // }
    }
}
