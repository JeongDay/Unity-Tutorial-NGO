using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkTilemap : NetworkBehaviour
{
    [SerializeField] private GameObject[] minerals; // 생성할 광물

    private Tilemap tilemap;

    // 파괴된 Tile의 Position을 저장하고 있는 동기화용 List
    private NetworkList<Vector3Int> destroyedTiles = new  NetworkList<Vector3Int>();
    
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        destroyedTiles.OnListChanged += OnTileDestroyed;

        // 늦게 들어온 Client에게 이미 파괴된 Tile을 동기화 해주기 위한 기능
        foreach (var tilePos in destroyedTiles)
        {
            tilemap.SetTile(tilePos, null);
        }
    }

    public void RemoveTile(Vector3 hitPos)
    {
        if (!IsServer)
            return;
        
        Vector3Int cellPos = tilemap.WorldToCell(hitPos);
        
        // 30% 확률의 랜덤 광물 드랍
        int ranItemDrop = Random.Range(0, 101);
        if (ranItemDrop >= 70)
        {
            int ranIndex = Random.Range(0, minerals.Length);
            
            GameObject mineral = Instantiate(minerals[ranIndex], cellPos, Quaternion.identity);
            mineral.GetComponent<NetworkObject>().Spawn();
        }

        if (tilemap.GetTile(cellPos) != null)
        {
            destroyedTiles.Add(cellPos);
        }
    }

    // 특정 위치가 파괴될 대상으로 Add된 경우, 해당 Tile을 삭제하는 기능 실행
    private void OnTileDestroyed(NetworkListEvent<Vector3Int> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<Vector3Int>.EventType.Add)
        {
            tilemap.SetTile(changeEvent.Value, null);
        }
    }
}