using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : NetworkBehaviour
{
    private Vector3 moveInput;

    void Update()
    {
        if (IsOwner)
            transform.position += moveInput * 3f * Time.deltaTime;
    }

    void OnMove(InputValue value)
    {
        var moveValue = value.Get<Vector2>();

        moveInput = new Vector3(moveValue.x, 0, moveValue.y);
    }
}