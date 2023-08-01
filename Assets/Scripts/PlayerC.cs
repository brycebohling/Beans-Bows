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

        if (currentArrow != null)
        {
            currentArrow.localRotation = Quaternion.identity;
        }

        isGrounded = IsGrounded();

        HandleHorizontalMovement();
        HandleJump();
        HandleBow();
    }

    private void FixedUpdate() 
    {   
        if (!IsOwner) return;

        rb.velocity = move;

        if (isArrowLerping)
        {
            currentArrow.position = Vector3.Lerp(arrowSpawnPos.position, stringBackPos.position, arrowTimeElapsed / arrowLerpDuration);
            arrowTimeElapsed += Time.deltaTime;

            if (arrowTimeElapsed >= arrowLerpDuration)
            {
                isArrowLerping = false;
                currentArrow.position = stringBackPos.position;
            }
        }
    }

    // private void SpawnBowArrow()
    // {
    //     bowArrowHolder = Instantiate(bowArrowHolderPrefab, bowArrowSpawnPos, Quaternion.identity);
    //     SpawnBowArrowServerRpc();
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SpawnBowArrowServerRpc()
    // {
    //     bowArrowHolderSenderTransform.someTransform.GetComponent<NetworkObject>()
    // }

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
                SpawnArrow();

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

    private void SpawnArrow()
    {
        currentArrow = Instantiate(arrowPrefab, arrowSpawnPos.position, Quaternion.identity);
        SpawnArrowServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnArrowServerRpc(ServerRpcParams serverRpcParams = default)
    {
        currentArrow = Instantiate(arrowPrefab, arrowSpawnPos.position, Quaternion.identity);
    
        currentArrow.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        // currentArrow.GetComponent<NetworkObject>().ChangeOwnership(serverRpcParams.Receive.SenderClientId);
        // SetParentClientRpc(new SerializeTransform {someTransform = currentArrow}, );
    }

    private void FireArrow(float arrowForce)
    {
        currentArrow.GetComponent<ArrowC>().ShootArrow(arrowForce);
        // RemoveParentServerRpc(new SerializeTransform {someTransform = currentArrow});
    }

    // [ServerRpc(RequireOwnership =  false)]
    // private void SetParentServerRpc()
    // {
    //     SetParentClientRpc(objectTransform, wantedParent);
    // }

    // [ClientRpc]
    // private void SetParentClientRpc()
    // {
    //     objectTransform.someTransform.GetComponent<NetworkObject>().TrySetParent(wantedParent.someTransform);  
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void RemoveParentServerRpc()
    // {
    //     RemoveParentClientRpc(objectTransform);
    // }

    // [ClientRpc]
    // private void RemoveParentClientRpc()
    // {
    //     objectTransform.someTransform.GetComponent<NetworkObject>().TryRemoveParent();
    // }

    // private bool IsAnimationPlaying(Animator animator, string stateName)
    // {
    //     if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
    //     {
    //         return true;
    //     } else
    //     {
    //         return false;
    //     }
    // }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(groundCheckTransform.position, groundCheckSize);
    }
}
