using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Biomes/BiomeData")]
[System.Serializable]
public class BiomeData : ScriptableObject
{
    public int biomeId; //Unique ID for biome
    public string biomeName;

    [Header("ObjectSpawn Settings")]
    public int minSpawnCount = 100;
    public int maxSpawnCount = 250;

    // Dictionary to store allowed tiles for each object
    [System.Serializable]
    public class ObjectTerrainSpawnRule
    {
        public GameObject objectToSpawn;
        public List<TileBase> allowedTiles;

        public bool requireSpacing;
        public List<string> cannotSpawnNear;
        public int spacingDistance = 1;

        public string objectType;
        public float rarity = 10; // Default rarity (1-50)

        // --- NEW FIELDS for cluster spawning ---
        [Header("Cluster Settings")]
        public bool useDenseClusters = false;
        public int clusterRadius = 3;    // how large each cluster is
        public int clusterCount = 1;     // how many separate clusters to spawn
    }

    public List<ObjectTerrainSpawnRule> spawnRules;
}
