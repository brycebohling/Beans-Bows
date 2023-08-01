using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArrowC : NetworkBehaviour
{
    [SerializeField] float dmg;    
    [SerializeField] float collisionLayerNum;
    Rigidbody rb;
    FollowTransform followTransformScript;


    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        followTransformScript = GetComponent<FollowTransform>();

        rb.useGravity = false;
    }

    public void ShootArrow(float arrowForce)
    {
        followTransformScript.SetTargetTransform(null);

        rb.useGravity = true;
        rb.AddForce(PlayerCamera.Instance.transform.forward * arrowForce, ForceMode.Impulse);

        RemoveArrowParentServerRpc(arrowForce, OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveArrowParentServerRpc(float arrowForce, ulong clientId)
    {
        RemoveArrowParentClientRpc(arrowForce, clientId);
    }

    [ClientRpc]
    private void RemoveArrowParentClientRpc(float arrowForce, ulong clientId)
    {
        if (IsOwner) return;
        
        followTransformScript.SetTargetTransform(null);
    }

    private void Update()
    {
        transform.LookAt(transform.position + rb.velocity);
    }

    private void OnTriggerEnter(Collider collision) 
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("hit player");
            // DestroyArrowServerRpc();
        } else if (collision.gameObject.layer == collisionLayerNum)
        {
            // DestroyArrowServerRpc();
        }
    }

    public void SetArrowParent(PlayerC arrowParent)
    {
        SetArrowParentServerRpc(arrowParent.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetArrowParentServerRpc(NetworkObjectReference arrowParentNetworkObjectReference)
    {    
        SetArrowParentClientRpc(arrowParentNetworkObjectReference);        
    }

    [ClientRpc]
    private void SetArrowParentClientRpc(NetworkObjectReference arrowParentNetworkObjectReference)
    {    
        arrowParentNetworkObjectReference.TryGet(out NetworkObject arrowParentNetworkObject);
        PlayerC arrowParentPlayerC = arrowParentNetworkObject.GetComponent<PlayerC>();

        followTransformScript.SetTargetTransform(arrowParentPlayerC.GetArrowFollowTransform());
    }

    // if (isArrowLerping && currentArrow != null)
    // {
    //     currentArrow.position = Vector3.Lerp(arrowSpawnPos.position, stringBackPos.position, arrowTimeElapsed / arrowLerpDuration);
    //     arrowTimeElapsed += Time.deltaTime;

    //     if (arrowTimeElapsed >= arrowLerpDuration)
    //     {
    //         isArrowLerping = false;
    //         currentArrow.position = stringBackPos.position;
    //     }
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void DestroyArrowServerRpc()
    // {
    //     gameObject.GetComponent<NetworkObject>().Despawn(true);
    //     Destroy(gameObject);
    // }

    
}
