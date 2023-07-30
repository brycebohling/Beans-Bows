using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArrowC : NetworkBehaviour
{
    [SerializeField] float dmg;    
    [SerializeField] float collisionLayerNum;
    Rigidbody rb;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void ShootArrow(float arrowForce)
    {
        rb.useGravity = true;
        rb.AddForce(PlayerCamera.Instance.transform.forward * arrowForce, ForceMode.Impulse);
    }

    private void Update()
    {
        if (!IsOwner) return;
        transform.LookAt(transform.position + rb.velocity);
    }

    private void OnTriggerEnter(Collider collision) 
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("hit player");
            DestroyArrowServerRpc();
        } else if (collision.gameObject.layer == collisionLayerNum)
        {
            DestroyArrowServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyArrowServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }

    
}
