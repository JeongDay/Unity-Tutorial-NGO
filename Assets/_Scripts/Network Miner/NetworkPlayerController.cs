using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    // 0: Idle, 1: Move, 2: Attack
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

        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }
    
    void Update()
    {
        if (IsOwner)
            Movement();
    }

    private void Movement()
    {
        if (currentAnimState.Value == 2)
            return;
        
        if (moveInput.x == 0)
        {
            currentAnimState.Value = 0;
        }
        else if (moveInput.x != 0)
        {
            currentAnimState.Value = 1;
            
            int dirX = moveInput.x < 0 ? 1 : -1;
            transform.localScale = new Vector3(dirX, 1, 1);
            
            transform.position += moveInput * moveSpeed * Time.deltaTime;
        }
    }

    void OnMove(InputValue value)
    {
        var moveValue = value.Get<Vector2>();

        moveInput = new Vector3(moveValue.x, 0, 0);
    }

    void OnJump()
    {
        if (IsOwner)
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