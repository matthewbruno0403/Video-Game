using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Tiles/Biome Tile")]
public class BiomeTile : Tile
{
    public int biomeID; // 1 = Forest 2 = Desert, etc.
    public string biomeName; // "Forest", "Desert"
}
