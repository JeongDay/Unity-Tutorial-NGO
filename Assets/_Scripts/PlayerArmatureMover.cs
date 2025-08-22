using StarterAssets;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerArmatureMover : NetworkBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterAsset;
    [SerializeField] private ThirdPersonController controller;
    [SerializeField] private Transform playerRoot;

    [SerializeField] private GameObject bombPrefab;
    
    void Awake()
    {
        cc.enabled = false;
        playerInput.enabled = false;
        starterAsset.enabled = false;
        controller.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            cc.enabled = true;
            playerInput.enabled = true;
            starterAsset.enabled = true;
            controller.enabled = true;

            var cinemachine = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineCamera>();
            cinemachine.Target.TrackingTarget = playerRoot;
        }
    }
    
    void Update()
    {
        if (!IsOwner)
            return;
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AddScoreServerRpc();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            ThrowBombServerRpc();
        }
    }
    
    [ServerRpc]
    private void ThrowBombServerRpc()
    {
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }

    [ServerRpc]
    private void AddScoreServerRpc()
    {
        ScoreManager.Instance.AddScore();
    }
}