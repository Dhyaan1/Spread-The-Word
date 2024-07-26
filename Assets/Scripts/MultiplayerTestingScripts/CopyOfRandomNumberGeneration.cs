using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CopyOfRandomNumberGeneration : NetworkBehaviour
{
    private NetworkVariable<int> RandomNumber = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        // Update points text when the network variable changes
        RandomNumber.OnValueChanged += (int oldValue, int newValue) => {
            Debug.Log("Owner: "+OwnerClientId +";Random Number: " + RandomNumber.Value);
        };
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!IsOwner) 
                {
                    Debug.Log("Owner: "+OwnerClientId + "is Not a Owner");
                }
            else
                {
                    Debug.Log("Owner: "+OwnerClientId + "is a Owner");
                }
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RandomNumber.Value = Random.Range(0, 100);
        }
    }
}
