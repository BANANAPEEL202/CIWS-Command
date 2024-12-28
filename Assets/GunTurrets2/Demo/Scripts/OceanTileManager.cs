using System.Collections.Generic;
using UnityEngine;

public class OceanTileManager : MonoBehaviour
{
    public GameObject oceanTilePrefab;  // Assign your ocean tile prefab
    public Transform player;            // Assign the player object
    public float tileSize = 10;         // Size of each tile (assuming square tiles)

    private Vector2Int currentTile;     // The current tile position of the player
    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        tileSize = oceanTilePrefab.GetComponent<MeshRenderer>().bounds.size.x;
        // Adjusting for center-based tile coordinates (subtracting half the tile size)
        UpdateGrid(new Vector2Int(
            (int)((player.position.x + tileSize / 2) / tileSize),
            (int)((player.position.z + tileSize / 2) / tileSize)
        ));
    }

    void Update()
    {
        // Determine player's current tile based on center of the tile
        int tilePositionX = (int)((player.position.x + tileSize / 2) / tileSize);
        if (player.position.x < 0)
        {
            tilePositionX = (int)((player.position.x - tileSize / 2) / tileSize);
        }
        int tilePositionZ = (int)((player.position.z + tileSize / 2) / tileSize);
        if (player.position.z < 0)
        {
            tilePositionZ = (int)((player.position.z - tileSize / 2) / tileSize);
        }
        Vector2Int playerTile = new Vector2Int(
            tilePositionX,
            tilePositionZ
        );
        
        // If the player has moved to a new tile, update the grid
        if (playerTile != currentTile)
        {
            UpdateGrid(playerTile);
        }
    }

    void UpdateGrid(Vector2Int newTile)
    {
        currentTile = newTile;

        // Create a 3x3 grid around the player, accounting for the center-based tile coordinates
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int tilePosition = newTile + new Vector2Int(x, z);

                if (!activeTiles.ContainsKey(tilePosition))
                {
                    // Spawn new tile at the center of the calculated position
                    Vector3 worldPosition = new Vector3(tilePosition.x * tileSize, 0, tilePosition.y * tileSize);
                    GameObject newTileObj = Instantiate(oceanTilePrefab, worldPosition, Quaternion.identity);
                    activeTiles.Add(tilePosition, newTileObj);
                }
            }
        }

        // Remove tiles that are no longer in the grid
        List<Vector2Int> tilesToRemove = new List<Vector2Int>();

        foreach (var tile in activeTiles.Keys)
        {
            if (Mathf.Abs(tile.x - newTile.x) > 1 || Mathf.Abs(tile.y - newTile.y) > 1)
            {
                tilesToRemove.Add(tile);
            }
        }

        foreach (var tile in tilesToRemove)
        {
            Destroy(activeTiles[tile]);
            activeTiles.Remove(tile);
        }
    }
}
