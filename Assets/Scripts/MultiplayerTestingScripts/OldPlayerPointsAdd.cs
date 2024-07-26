using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class OldPlayerPointsAdd : NetworkBehaviour
{
    [SerializeField] private Button PlayerAddPoints;
    [SerializeField] private TextMeshProUGUI PlayerPoints;
    [SerializeField] private TextMeshProUGUI OpponentPoints;
    private NetworkVariable<int> Player1PointsValue = new NetworkVariable<int>(0);
    private NetworkVariable<int> Player2PointsValue = new NetworkVariable<int>(0);

    void Start()
    {
        // Update points text when the network variable changes
        Player1PointsValue.OnValueChanged += (oldValue, newValue) => UpdatePointsText();
        Player2PointsValue.OnValueChanged += (oldValue, newValue) => UpdatePointsText();

        // Add points when the button is clicked
        PlayerAddPoints.onClick.AddListener(() =>
        {
            AddPointServerRpc();
        });
    }

    [ServerRpc(RequireOwnership = false)]
    void AddPointServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId == OwnerClientId)
        {
            Player1PointsValue.Value += 1;
        }
        else
        {
            Player2PointsValue.Value += 1;
        }

        // Call the ClientRpc to update the UI on the client
        UpdateButtonClientRpc();
    }

    [ClientRpc]
    void UpdateButtonClientRpc()
    {
        // Enable the button on the client
        PlayerAddPoints.interactable = true;
    }

    void UpdatePointsText()
    {
        if (IsOwner)
        {
            PlayerPoints.text = Player1PointsValue.Value.ToString();
            OpponentPoints.text = Player2PointsValue.Value.ToString();
        }
        else
        {
            PlayerPoints.text = Player2PointsValue.Value.ToString();
            OpponentPoints.text = Player1PointsValue.Value.ToString();
        }
    }
}