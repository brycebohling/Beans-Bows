using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerC : NetworkBehaviour
{
    public static PlayerC Instance { get; private set; }

    Rigidbody rb;

    // Movement
    Vector3 move;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundCheckTransform;
    [SerializeField] float groundCheckSize;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;
    float gravityValue = -9.81f;
    [SerializeField] float gravityScale;

    // Arrow
    [SerializeField] Transform arrowTransform;
    [SerializeField] Transform arrowSpawnPoint;

    
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Instance = this;

        Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;

        isGrounded = IsGrounded();

        HandleHorizontalMovement();
        HandleJump();
        HandleArrow();
    }

    private void FixedUpdate() 
    {
        rb.velocity = move;
    }

    private void HandleHorizontalMovement()
    {
        move = new Vector3(Input.GetAxisRaw("Horizontal") * moveSpeed, 0, Input.GetAxisRaw("Vertical") * moveSpeed);
        
        move = transform.forward * move.z + transform.right * move.x;

        move.y = rb.velocity.y + gravityValue * gravityScale * Time.deltaTime;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheckTransform.position, groundCheckSize, groundLayer);
    }

    private void HandleArrow()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ShootArrowServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootArrowServerRpc()
    {
        Transform arrow = Instantiate(arrowTransform, arrowSpawnPoint.position, Quaternion.identity);
        // arrow.LookAt(transform.position, Vector3.up);

        arrow.GetComponent<NetworkObject>().Spawn(true);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(groundCheckTransform.position, groundCheckSize);
    }
}
