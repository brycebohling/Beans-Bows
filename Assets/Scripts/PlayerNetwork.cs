using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<MyCostomData> randomNumber = new NetworkVariable<MyCostomData>(
        new MyCostomData {
            _int = 56,
            _bool = true,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCostomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCostomData previousValue, MyCostomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }
    private void Update() 
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            // TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> {1} } });
            // randomNumber.Value = new MyCostomData {
            //     _int = 10,
            //     _bool = false,
            //     message = "All your base are belong to us!",
            // };
        }

        // if (Input.GetKeyDown(KeyCode.Y) && spawnedObjTransform != null)
        // {
            // spawnedObjTransform.GetComponent<NetworkObject>().Despawn(true);
        //     Destroy(spawnedObjTransform.gameObject);
        // }   
    }

    [ServerRpc()]
    private void TestServerRpc() 
    {
        
    }
    
    [ClientRpc()]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("Test server RPC" + clientRpcParams);
    }
}