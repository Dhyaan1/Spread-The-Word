using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class OneVOnePointGamePointSystem : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI Player1Points;
    [SerializeField] private TextMeshProUGUI Player2Points;
    private int Player1PointsValue;
    private int Player2PointsValue;

    private ulong MyClientId;


    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            Player1PointsValue = 0;
            Player2PointsValue = 0;
            UpdatePointsText();
        }

        MyClientId = NetworkManager.Singleton.LocalClientId;
    }

    public ulong GetLocalPlayerClientId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public ulong[] GetAllClientIds()
    {
        return NetworkManager.Singleton.ConnectedClientsIds.ToArray();
    }

    void UpdatePointsText()
    {
        if (IsHost)
        {
            Player1Points.text = Player1PointsValue.ToString();
            Player2Points.text = Player2PointsValue.ToString();
        }
        else
        {
            Player2Points.text = Player1PointsValue.ToString();
            Player1Points.text = Player2PointsValue.ToString();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // AddPointServerRpc();
            if (IsHost)
            {
                AddPointToPlayer1Rpc();
                Debug.Log("Server" + OwnerClientId);
                Debug.Log("Actual Server" + NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                AddPointToPlayer2Rpc();
                Debug.Log("Client" + OwnerClientId);
                Debug.Log("Actual Client" + NetworkManager.Singleton.LocalClientId);
            }
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
    void AddPointToPlayer2Rpc()
    {
        Player2PointsValue++;
        UpdatePointsText();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AddPointToPlayer1Rpc()
    {
        Player1PointsValue++;
        UpdatePointsText();
    }

}