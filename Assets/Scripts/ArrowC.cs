using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArrowC : NetworkBehaviour
{
    [SerializeField] float dmg;    
    // [SerializeField] float groundLayerNum;
    Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
    }

    public void ShootArrow(float arrowForce)
    {
        rb.useGravity = true;

        rb.AddForce(PlayerCamera.Instance.transform.forward * arrowForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter3D(Collider collision) 
    {
        // if (collision.gameObject.CompareTag("Player") && !GameManager.gameManager.isPLayerInvicible)
        // {
        //     GameManager.gameManager.DamagePlayer(dmg, transform);
        //     Destroy(gameObject);
        // } else if (collision.gameObject.layer == groundLayerNum)
        // {
        //     Destroy(gameObject);
        // }
        
    }
}
