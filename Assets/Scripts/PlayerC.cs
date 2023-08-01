using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerC : NetworkBehaviour
{
    public static PlayerC Instance { get; private set; }

    Rigidbody rb;

    [Header("Movement")]
    Vector3 move;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundCheckTransform;
    [SerializeField] float groundCheckSize;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;
    float gravityValue = -9.81f;
    [SerializeField] float gravityScale;

    [Header("Bow")]
    public Transform BowArrowHolder;
    [SerializeField] Transform bow;
    [SerializeField] Transform stringBackPos;
    Animator bowAnim;
    const string BOW_RELEASE = "Release";
    const string BOW_DRAW_BACK = "DrawBack";
    float drawBackTime = 1.5f;
    float drawBackElaped;
    bool isBowDrawingBack;

    [Header("Arrow")]
    [SerializeField] Transform arrowPrefab;
    [SerializeField] Transform arrowSpawnPos;
    Transform currentArrow;
    bool isArrowLerping;
    float arrowTimeElapsed;
    float arrowLerpDuration;
    float arrowSpeed = 20f;

    
    // public override void OnNetworkSpawn()
    // {
    //     if (!IsOwner) return;

    //     Instance = this;
    //     Cursor.lockState = CursorLockMode.Locked;
    //     rb = GetComponent<Rigidbody>();

    //     bowAnim = bow.GetComponent<Animator>();
    // }

    private void Start()
    {
        if (!IsOwner) return;

        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();

        bowAnim = bow.GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;

        isGrounded = IsGrounded();

        HandleHorizontalMovement();
        HandleJump();
        HandleBow();
    }

    private void FixedUpdate() 
    {   
        if (!IsOwner) return;

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

    private void HandleBow()
    {   
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            float arrowForce = drawBackElaped / drawBackTime * arrowSpeed;

            FireArrow(arrowForce);

            isBowDrawingBack = false;
            isArrowLerping = false;
            arrowTimeElapsed = 0;

            bowAnim.Play(BOW_RELEASE);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            drawBackElaped += Time.deltaTime;

            if (drawBackElaped > drawBackTime)
            {
                drawBackElaped = drawBackTime;
            }

            if (!isBowDrawingBack)
            {
                SpawnArrow(this);

                isBowDrawingBack = true;

                bowAnim.Play(BOW_DRAW_BACK);

            } else if (!isArrowLerping)
            {
                isArrowLerping = true;
                arrowLerpDuration = bowAnim.GetCurrentAnimatorStateInfo(0).length;
            }

        } else
        {
            drawBackElaped = 0;
        }
    }

    private void SpawnArrow(PlayerC arrowParent)
    {
        SpawnArrowServerRpc(arrowParent.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnArrowServerRpc(NetworkObjectReference arrowParentNetworkObjectReference)
    {    
        Transform arrow = Instantiate(arrowPrefab, arrowSpawnPos.position, Quaternion.identity);
        arrow.GetComponent<NetworkObject>().Spawn(true);

        arrowParentNetworkObjectReference.TryGet(out NetworkObject arrowParentNetworkObject);
    
        arrow.GetComponent<NetworkObject>().ChangeOwnership(arrowParentNetworkObject.OwnerClientId);

        arrow.GetComponent<ArrowC>().SetArrowParent(this);

        SetCurrentArrowReferenceClientRpc(arrow.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void SetCurrentArrowReferenceClientRpc(NetworkObjectReference arrowNetworkObjectReference)
    {
        if (!IsOwner) return;
        
        arrowNetworkObjectReference.TryGet(out NetworkObject arrowNetworkObject);
        currentArrow = arrowNetworkObject.GetComponent<Transform>();
    }

    private void FireArrow(float arrowForce)
    {
        if (currentArrow == null) return;
        
        currentArrow.GetComponent<ArrowC>().ShootArrow(arrowForce);
    }

    public Transform GetArrowFollowTransform()
    {
        return BowArrowHolder;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(groundCheckTransform.position, groundCheckSize);
    }
}
