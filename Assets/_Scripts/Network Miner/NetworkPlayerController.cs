using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    private NetworkVariable<int> currentAnimState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private GameObject[] animObjs;
    private Rigidbody2D rb;

    private Vector3 moveInput;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpPower = 7f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        rb = GetComponent<Rigidbody2D>();
        currentAnimState.OnValueChanged += UpdateAnimation;

        if (!IsServer)
        {
            rb.isKinematic = true;
        }

        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
        }
        else
        {
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

            if (cameraFollow != null)
            {
                cameraFollow.target = transform;
            }
        }
    }

    void Update()
    {
        if (IsOwner)
            MovementServerRpc(moveInput);
    }

    [ServerRpc]
    private void MovementServerRpc(Vector2 moveDir)
    {
        if (currentAnimState.Value == 2)
            return;

        if (moveDir.x == 0)
        {
            currentAnimState.Value = 0;
        }
        else if (moveDir.x != 0)
        {
            rb.linearVelocity = new Vector2(moveDir.x * moveSpeed, rb.linearVelocity.y);
            
            int dirX = moveDir.x < 0 ? 1 : -1;
            transform.localScale = new Vector3(dirX, 1, 1);

            currentAnimState.Value = 1;
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump()
    {
        if (IsOwner)
            JumpServerRpc();
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        rb.AddForceY(jumpPower, ForceMode2D.Impulse);
    }

    void OnAttack()
    {
        if (IsOwner)
        {
            if (currentAnimState.Value != 2)
                AttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        currentAnimState.Value = 2;

        yield return new WaitForSeconds(1f);
        currentAnimState.Value = 0;
    }
    
    private void UpdateAnimation(int prevValue, int newValue)
    {
        for (int i = 0; i < animObjs.Length; i++)
            animObjs[i].SetActive(i == newValue);
    }
}