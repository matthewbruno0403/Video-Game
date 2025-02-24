using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Highlight Tile")]
public class HighlightTile : Tile
{
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        
        // Ensure transparency by setting the tile's color
        tileData.color = new Color(1f, 1f, 1f, 0.9f); // 90% visible, slightly transparent
    }
}