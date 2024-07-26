using TMPro;
using UnityEngine;
using Unity.Netcode;

public class TwoVTwoPlayerGamePointSystem : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerName;
    [SerializeField] private TextMeshProUGUI Player1Points;
    [SerializeField] private TextMeshProUGUI Player2Points;
    [SerializeField] private TextMeshProUGUI Player3Points;
    [SerializeField] private TextMeshProUGUI Player4Points;
    private NetworkList<int> PlayerPoints;
    private NetworkList<ulong> AllClientIds;
    private ulong MyClientId; // Only needed for checking if client is on the list of all client IDs
    private bool initialTextUpdate = false; // Temp- This is used to update the text only once when the player joins the game

    void Awake()
    {
        PlayerPoints = new NetworkList<int>(new int[4]);
        AllClientIds = new NetworkList<ulong>();
    }

    // We should also update the rest of this code to handle a dynamic number of players.
    // For example, the UpdatePointsText method currently assumes that there are always four players.
    // If we want to support a dynamic number of players, we might need to
    // create the TextMeshProUGUI objects for the player points dynamically.

    public override void OnNetworkSpawn()
    {
        // Cache the local client ID so as to not call NetworkManager.Singleton.LocalClientId multiple times
        MyClientId = NetworkManager.Singleton.LocalClientId;
        UpdateAllClientIdsRpc(); // Update the list of all client IDs when a new player joins
    }

    // NetworkList has an add function so we can add as many players as we want
    [Rpc(SendTo.Server)]
    void UpdateAllClientIdsRpc()
    {
        // If Statement probably not needed after making UI dynamic
        // So that we can add as many players as we want without an Index error in the curernt UI setup
        if(AllClientIds.Count >= 4) 
        {
            PlayerPoints.Add(0);
        }

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!AllClientIds.Contains(clientId))
            {
                AllClientIds.Add(clientId);
            }
        }
    }

    // Temp- This is used to update the text only once when the player joins the game
    // Not needed after setting up a lobby loading scene
    void InitialUpdatePointsText() 
    {
        if(AllClientIds.IndexOf(MyClientId) == -1)
        {
            return;
        }
        initialTextUpdate = true;
        PlayerName.text = "Player " + (AllClientIds.IndexOf(MyClientId)+1).ToString();
        Player1Points.text = PlayerPoints[0].ToString();
        Player2Points.text = PlayerPoints[1].ToString();
        Player3Points.text = PlayerPoints[2].ToString();
        Player4Points.text = PlayerPoints[3].ToString();
    }
    void UpdatePointsText()
    {
        if(AllClientIds.IndexOf(MyClientId) == -1)
        {
            return;
        }

        // Temp - replace with the Username rather than Player 1, Player 2, etc.
        PlayerName.text = "Player " + (AllClientIds.IndexOf(MyClientId)+1).ToString(); 

        // Temp - replace with dynamic UI setup
        Player1Points.text = PlayerPoints[0].ToString();
        Player2Points.text = PlayerPoints[1].ToString();
        Player3Points.text = PlayerPoints[2].ToString();
        Player4Points.text = PlayerPoints[3].ToString();
    }

    private void Start()
    {
        // Update points text when the network list changes
        PlayerPoints.OnListChanged += (list) => UpdatePointsText();
    }

    private void Update()
    {
        // Dont allow player to add points if they are not in the list of all client IDs
        // Not needed as also checked in the AddPointToPlayerRpc method
        // if(AllClientIds.IndexOf(MyClientId) == -1)
        // {
        //     return;
        // }

        if(!initialTextUpdate) // Check every frame if the text has been updated
        {
            InitialUpdatePointsText();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddPointToPlayerRpc();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    [Rpc(SendTo.Server)]
    void AddPointToPlayerRpc(RpcParams rpcParams = default)
    {
        int index = AllClientIds.IndexOf(rpcParams.Receive.SenderClientId);
        if (index != -1)
        {
            if (PlayerPoints == null)
            {
                Debug.LogError("PlayerPoints is null");
                return;
            }
            PlayerPoints[index]++;
            LogPlayerPoints();
            UpdatePointsText();
        }
    }

        // void LogAllClientIds()
    // {
    //     string clientIds = "";
    //     for (int i = 0; i < AllClientIds.Count; i++)
    //     {
    //         clientIds += AllClientIds[i].ToString();
    //         if (i < AllClientIds.Count - 1)
    //         {
    //             clientIds += ", ";
    //         }
    //     }
    //     Debug.Log("AllClientIds: " + clientIds);
    // }

    void LogPlayerPoints()
    {
        string points = "";
        for (int i = 0; i < PlayerPoints.Count; i++)
        {
            points += PlayerPoints[i].ToString();
            if (i < PlayerPoints.Count - 1)
            {
                points += ", ";
            }
        }
        Debug.Log("PlayerPoints: " + points);
    }

}
