using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject[] animObjs;
    
    private Rigidbody2D rb;
    
    private Vector3 moveInput;
    
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpPower = 7f;

    private bool isAttack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isAttack)
            return;
        
        if (moveInput.x == 0)
        {
            SetAnimObject(0);
        }
        else if (moveInput.x != 0)
        {
            SetAnimObject(1);
            
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
        rb.AddForceY(jumpPower, ForceMode2D.Impulse);
    }

    void OnAttack()
    {
        if (!isAttack)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttack = true;
        SetAnimObject(2);

        yield return new WaitForSeconds(1f);
        SetAnimObject(0);
        isAttack = false;
    }

    private void SetAnimObject(int index)
    {
        for (int i = 0; i < animObjs.Length; i++)
            animObjs[i].SetActive(false);
        
        animObjs[index].SetActive(true);
    }
}