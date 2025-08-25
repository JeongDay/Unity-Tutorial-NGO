using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    // 동기화용 상태변화 변수
    private NetworkVariable<int> currentAnimState = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 애니메이션 오브젝트 배열
    [SerializeField] private GameObject[] animObjs;
    private Rigidbody2D rb;

    // 키보드 입력값
    private Vector3 moveInput;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpPower = 7f;

    // 네트워크 상에 생성될 때 실행
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        rb = GetComponent<Rigidbody2D>();
        currentAnimState.OnValueChanged += UpdateAnimation; // 상태가 변경될 때 UpdateAnimation 함수 실행

        /// Server가 아닌 Client에게는 Kinematic을 적용하여 Client 상에서 움직이지 못하도록 설정
        /// 해당 코드는 Server 기준으로 작동되는 것이기 때문에 Server에서 움직임을 계산해서 전달
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
            MovementServerRpc(moveInput); // Client에서 자신의 입력값 전달
    }

    [ServerRpc]
    private void MovementServerRpc(Vector2 moveDir) // 서버에서 계산하는 움직임
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
    
    // 애니메이션 오브젝트 변경 함수
    private void UpdateAnimation(int prevValue, int newValue)
    {
        for (int i = 0; i < animObjs.Length; i++)
            animObjs[i].SetActive(i == newValue);
    }
}