using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkTilemap : MonoBehaviour
{
    [SerializeField] private GameObject[] minerals;

    private Tilemap tilemap;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void RemoveTile(Vector3 hitPos)
    {
        Vector3Int cellPos = tilemap.WorldToCell(hitPos);
        
        // Item Drop
        int ranItemDrop = Random.Range(0, 101);
        if (ranItemDrop >= 70)
        {
            int ranIndex = Random.Range(0, minerals.Length);
            Instantiate(minerals[ranIndex], cellPos, Quaternion.identity);
        }
        
        tilemap.SetTile(cellPos, null);
    }
}