using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBiomeDetection : MonoBehaviour
{
    public BiomeManager biomeManager; // Reference to BiomeManager

    public string GetCurrentBiome()
    {
        if (biomeManager != null)
        {
            BiomeData biome = biomeManager.GetBiomeData(transform.position);
            return biome != null ? biome.biomeName : "Void";
        }
        return "Unknown";
    }
}