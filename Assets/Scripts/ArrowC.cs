using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowC : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float dmg;    
    // [SerializeField] float groundLayerNum;
    Rigidbody rb;
    


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.right * speed;
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
