using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelector : MonoBehaviour
{
    public Tilemap tilemap; // The main terrain tilemap
    public Tilemap highlightTilemap; // New highlight tilemap
    public Tile highlightTile;
    private Vector3Int previousTilePos;
    private bool hasPreviousTile = false;

    void Update()
    {
        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Ensure it's in the correct plane

        // Convert world position to tilemap grid position
        Vector3Int tilePos = tilemap.WorldToCell(mouseWorldPos);

        if (tilePos == previousTilePos) return; // Avoid redundant updates

        // Clear the previous highlight tile
        if (hasPreviousTile)
        {
            highlightTilemap.SetTile(previousTilePos, null);
        }

        // Highlight the new tile only if it exists
        hasPreviousTile = tilemap.HasTile(tilePos);
        if (hasPreviousTile)
        {
            highlightTilemap.SetTile(tilePos, highlightTile);
        }

        previousTilePos = tilePos;
    }
}
