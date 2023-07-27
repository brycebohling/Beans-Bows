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

    // Bow
    [SerializeField] Transform bowHolder;
    [SerializeField] Transform bow;
    [SerializeField] Transform stringBackPos;
    Animator bowAnim;
    const string BOW_RELEASE = "Release";
    const string BOW_DRAW_BACK = "DrawBack";

    float drawBackCount = 1.5f;
    float drawBackTimer;

    bool isBowDrawnBack;

    // Arrow
    [SerializeField] Transform arrowPrefab;
    [SerializeField] Transform arrowSpawnPoint;
    Transform currentArrow;

    float arrowTimeElapsed;
    float arrowLerpDuration;

    float arrowSpeed = 20f;

    
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Instance = this;

        Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();

        bowAnim = bow.GetComponent<Animator>();

        // AnimationClip[] clips = bowAnim.runtimeAnimatorController.animationClips;
        // arrowLerpDuration = clips[BOW_DRAW_BACK].length;
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
            float arrowForce = drawBackTimer / drawBackCount * arrowSpeed;

            FireArrow(arrowForce);

            isBowDrawnBack = false;
            arrowTimeElapsed = 0;

            bowAnim.Play(BOW_RELEASE);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            drawBackTimer += Time.deltaTime;

            if (drawBackTimer > drawBackCount)
            {
                drawBackTimer = drawBackCount;
            }

            if (!isBowDrawnBack)
            {
                SpawnArrowServerRpc();

                isBowDrawnBack = true;

                bowAnim.Play(BOW_DRAW_BACK);
            } else if (arrowTimeElapsed < arrowLerpDuration)
            {    
                currentArrow.position = Vector3.Lerp(currentArrow.position, stringBackPos.position, arrowTimeElapsed / arrowLerpDuration);
                arrowTimeElapsed += Time.deltaTime;   
            }

        } else
        {
            drawBackTimer = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnArrowServerRpc()
    {
        currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, transform.rotation, bowHolder);
        
        currentArrow.GetComponent<NetworkObject>().Spawn(true);
        
        // clinentRpc
    }

    private void FireArrow(float arrowForce)
    {
        currentArrow.GetComponent<ArrowC>().ShootArrow(arrowForce);
    }

    private bool IsAnimationPlaying(Animator animator, string stateName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(groundCheckTransform.position, groundCheckSize);
    }
}
