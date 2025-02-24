using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeManager : MonoBehaviour
{
    public static BiomeManager instance;
    
    public Tilemap tilemap; // Assign this in Unity Inspector
    public List<BiomeData> biomeList;

    public void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    public BiomeData GetBiomeData(Vector3 worldPosition)
    {
        Vector3Int tilePosition = tilemap.WorldToCell(worldPosition); // Convert world position to tile position
        TileBase tile = tilemap.GetTile(tilePosition); // Get the tile at this position

        if (tile == null)
        {
            return null;
        }
        
        if (tile is BiomeTile biomeTile) // Check if it's a BiomeTile
        {
            foreach (BiomeData biome in biomeList)
            {
                if (biome.biomeId == biomeTile.biomeID) // Fix biomeID reference
                {
                    return biome;
                }
            }
        }
        return null; // Return null if no biome found
    }
    
    public BiomeData GetBiomeByID(int biomeID)
    {
        foreach (BiomeData biome in biomeList)
        {
            if (biome.biomeId == biomeID) // Ensure `biomeId` exists in your BiomeData class
            {
                return biome;
            }
        }
        return null;
    }
}