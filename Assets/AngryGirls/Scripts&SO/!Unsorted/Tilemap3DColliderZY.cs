using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Tilemap3DColliderZY : MonoBehaviour
{
    [ContextMenu("Generate ZY Colliders")]
    public void GenerateColliders()
    {
        // Clear existing generated 3D colliders first
        BoxCollider[] existingColliders = GetComponents<BoxCollider>();
        foreach (var c in existingColliders)
        {
            DestroyImmediate(c);
        }

        Tilemap tilemap = GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        Vector3 cellSize = tilemap.cellSize;

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x + z * bounds.size.x * bounds.size.y];
                    if (tile != null)
                    {
                        // Calculate local cell position based on ZY mapping variables
                        Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, bounds.zMin + z);
                        Vector3 localCenter = tilemap.GetCellCenterLocal(cellPos);

                        // Attach a 3D Box Collider
                        BoxCollider box = gameObject.AddComponent<BoxCollider>();

                        // ZY Orientation mapping: depth is X, width/height map to Z and Y
                        box.center = localCenter;
                        box.size = new Vector3(cellSize.z, cellSize.y, cellSize.x);
                    }
                }
            }
        }
        Debug.Log("Generated 3D Colliders for ZY Tilemap successfully.");
    }
}
