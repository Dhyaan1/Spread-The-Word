using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;

public class FourPlayerGamePointSystem : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerName;
    [SerializeField] private TextMeshProUGUI Player1Points;
    [SerializeField] private TextMeshProUGUI Player2Points;
    [SerializeField] private TextMeshProUGUI Player3Points;
    [SerializeField] private TextMeshProUGUI Player4Points;
    private int[] PlayerPoints= new int[4];

    private ulong MyClientId;
    private ulong[] AllClientIds;

    public override void OnNetworkSpawn()
    {
        UpdatePointsText();
        MyClientId = NetworkManager.Singleton.LocalClientId;
        GetAllClientIds();
        PlayerName.text = "Player " + (Array.IndexOf(AllClientIds, MyClientId)+1).ToString();
        SyncClientListCallFromServerRpc();
        if(IsHost)
        {
            InvokeRepeating(nameof(SyncPlayerPointsWrapper), 10f, 10f);
        }
    }

    public ulong GetLocalPlayerClientId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public ulong[] GetAllClientIds()
    {
        return AllClientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
    }

    void UpdatePointsText()
    {
        
            Player1Points.text = PlayerPoints[0].ToString();
            Player2Points.text = PlayerPoints[1].ToString();
            Player3Points.text = PlayerPoints[2].ToString();
            Player4Points.text = PlayerPoints[3].ToString();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddPointToPlayerRpc(MyClientId);
            Debug.Log("Space pressed");
        }

        if (Input.GetKeyDown(KeyCode.A)&&IsHost)
        {
            Debug.Log("All Clients: " + string.Join(", ", GetAllClientIds()));
        }

        if (Input.GetKeyDown(KeyCode.B)&&!IsHost)
        {
            Debug.Log("From Not Host- My ClientId: " + MyClientId);
        }

        if (Input.GetKeyDown(KeyCode.C)&&IsHost)
        {
            Debug.Log("From Host- My ClientId: " + MyClientId);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("From Anyone- My ClientId: " + GetLocalPlayerClientId());
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // Disconnect client
            if (NetworkManager.Singleton.IsServer)
            {
                // NetworkManager.Singleton.DisconnectClient(MyClientId);
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AddPointToPlayerRpc(ulong clientId)
    {
        Debug.Log("AddPointToPlayerRpc called");
        int index = Array.IndexOf(AllClientIds, clientId);
        if (index != -1)
        {
            if (PlayerPoints == null)
            {
                Debug.LogError("PlayerPoints is null");
                return;
            }
            PlayerPoints[index]++;
            Debug.Log("PlayerPoints: " + string.Join(", ", PlayerPoints));
            UpdatePointsText();
        }
    }

    [Rpc(SendTo.Server)]
    void SyncClientListCallFromServerRpc()
    {
        Debug.Log("SyncClientListCallFromServerRpc called");
        SyncClientListRpc(GetAllClientIds());
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncClientListRpc(ulong[] ClientList)
    {
        Debug.Log("SyncClientListRpc called");
        AllClientIds = ClientList;
        Debug.Log("AllClientIds: " + string.Join(", ", AllClientIds));
    }


    void SyncPlayerPointsWrapper()
    {
        SyncPlayerPointsRpc(PlayerPoints);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncPlayerPointsRpc(int[] PlayerPointsList)
    {
        Debug.Log("SyncPlayerPointsRpc called");
        PlayerPoints = PlayerPointsList;
        Debug.Log("PlayerPoints: " + string.Join(", ", PlayerPoints));
        UpdatePointsText();
    }
}