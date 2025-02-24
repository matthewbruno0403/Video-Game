using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour
{
    public Tilemap tilemap; // The tilemap you’ve drawn manually
    public BiomeManager biomeManager;

    // We no longer call SpawnObjects() in Start() 
    // if we only want chunk-based spawning:
    void Start()
    {
        // comment out if you only spawn chunk-by-chunk
        //SpawnObjects();
    }

    /// <summary>
    /// Spawn objects only within areaBounds. Returns list of spawned GameObjects.
    /// </summary>
    public List<GameObject> SpawnObjectsInBounds(BoundsInt areaBounds)
    {
        List<GameObject> spawnedObjects = new List<GameObject>();
        Dictionary<Vector3Int, string> occupiedPositions = new Dictionary<Vector3Int, string>();

        foreach (BiomeData biome in biomeManager.biomeList)
        {
            // Gather valid tiles for this biome only within areaBounds
            List<Vector3Int> validBiomeTiles = new List<Vector3Int>();
            foreach (Vector3Int pos in areaBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null && tile is BiomeTile bTile && bTile.biomeID == biome.biomeId)
                {
                    validBiomeTiles.Add(pos);
                }
            }

            if (validBiomeTiles.Count == 0)
                continue; // No tiles of that biome in this region

            int spawnCount = Random.Range(biome.minSpawnCount, biome.maxSpawnCount);
            spawnCount = Mathf.Min(spawnCount, validBiomeTiles.Count);

            int totalSpawned = 0;

            // 1) total rarity sum
            float totalRaritySum = 0;
            foreach (var rule in biome.spawnRules)
            {
                if (rule.rarity > 0) totalRaritySum += rule.rarity;
            }

            // 2) guaranteed spawns
            List<BiomeData.ObjectTerrainSpawnRule> guaranteedSpawns = new List<BiomeData.ObjectTerrainSpawnRule>();
            foreach (var rule in biome.spawnRules)
            {
                if (rule.rarity > 0) guaranteedSpawns.Add(rule);
            }

            // 3) spawn guaranteed
            //    (We assume "guaranteed" means we do at least 1 spawn per rule,
            //     but if you want different logic, adjust as needed.)
            foreach (var rule in guaranteedSpawns)
            {
                if (rule.useDenseClusters)
                {
                    // NEW: If rule uses clusters, spawn an entire cluster
                    List<GameObject> clusterSpawned = SpawnClusterOfObjects(
                        rule, occupiedPositions, validBiomeTiles
                    );
                    spawnedObjects.AddRange(clusterSpawned);
                    totalSpawned += clusterSpawned.Count;
                }
                else
                {
                    // Normal single spawn
                    GameObject g = SpawnObject(rule, occupiedPositions, validBiomeTiles);
                    if (g != null)
                    {
                        totalSpawned++;
                        spawnedObjects.Add(g);
                    }
                }
            }

            // 4) Weighted probability
            int attemptCount = 0;
            int attemptLimit = 2000;

            while (totalSpawned < spawnCount && attemptCount < attemptLimit)
            {
                attemptCount++;
                float randomValue = Random.Range(0, totalRaritySum);
                float runningTotal = 0;
                BiomeData.ObjectTerrainSpawnRule selectedRule = null;

                foreach (var rule in biome.spawnRules)
                {
                    runningTotal += rule.rarity;
                    if (randomValue < runningTotal)
                    {
                        selectedRule = rule;
                        break;
                    }
                }

                if (selectedRule != null)
                {
                    if (selectedRule.useDenseClusters)
                    {
                        // NEW: Spawn a cluster instead of a single object
                        List<GameObject> clusterSpawned = SpawnClusterOfObjects(
                            selectedRule, occupiedPositions, validBiomeTiles
                        );
                        if (clusterSpawned.Count > 0)
                        {
                            totalSpawned += clusterSpawned.Count;
                            spawnedObjects.AddRange(clusterSpawned);
                        }
                    }
                    else
                    {
                        // Normal single-object spawn
                        GameObject g = SpawnObject(selectedRule, occupiedPositions, validBiomeTiles);
                        if (g != null)
                        {
                            totalSpawned++;
                            spawnedObjects.Add(g);
                        }
                    }
                }
            }
        }

        return spawnedObjects;
    }

    /// <summary>
    /// Attempt to spawn a single object from rule on a tile from validBiomeTiles.
    /// If successful, returns the spawned GameObject; otherwise null.
    /// </summary>
    GameObject SpawnObject(
        BiomeData.ObjectTerrainSpawnRule rule,
        Dictionary<Vector3Int, string> occupiedPositions,
        List<Vector3Int> validBiomeTiles)
    {
        // Gather all tiles that match rule.allowedTiles
        List<Vector3Int> validTiles = new List<Vector3Int>();
        foreach (var pos in validBiomeTiles)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null && rule.allowedTiles.Contains(tile))
            {
                validTiles.Add(pos);
            }
        }

        if (validTiles.Count == 0) return null;

        // Pick a random tile
        Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];

        // Check spacing
        if (rule.requireSpacing &&
            IsNearOtherObject(spawnTile, occupiedPositions, rule.cannotSpawnNear, rule.spacingDistance))
        {
            return null; 
        }

        // If tile is free, spawn object
        if (!occupiedPositions.ContainsKey(spawnTile))
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(spawnTile);
            GameObject spawnedObject = Instantiate(rule.objectToSpawn, worldPos, Quaternion.identity);

            // Mark it occupied
            ObjectType objTypeComponent = spawnedObject.GetComponent<ObjectType>();
            occupiedPositions[spawnTile] = objTypeComponent != null ? 
                                           objTypeComponent.objectType : 
                                           rule.objectType;

            
            // Register with TileObjectManager ---
            Vector2Int tileCoord = new Vector2Int(spawnTile.x, spawnTile.y);
            ToolHit[] toolHit = spawnedObject.GetComponentsInChildren<ToolHit>();
            foreach (ToolHit th in toolHit)
            {
                // Store it on the object
                th.tileCoord = tileCoord;
                // Register
                TileObjectManager.Instance.RegisterObject(tileCoord, th);
            }
            
            // Adjust sorting
            SpriteRenderer sr = spawnedObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = Mathf.RoundToInt(-worldPos.y * 100);

            return spawnedObject;
        }

        return null;
    }

        List<GameObject> SpawnClusterOfObjects(
        BiomeData.ObjectTerrainSpawnRule rule,
        Dictionary<Vector3Int, string> occupiedPositions,
        List<Vector3Int> validBiomeTiles)
    {
        List<GameObject> clusterObjects = new List<GameObject>();

        // For each cluster we want to spawn
        for (int c = 0; c < rule.clusterCount; c++)
        {
            // 1) Pick a random tile as the "center" of this cluster
            Vector3Int centerTile = validBiomeTiles[Random.Range(0, validBiomeTiles.Count)];

            // 2) For each tile in a radius around centerTile
            int r = rule.clusterRadius;
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    // --- CHANGED: Skip tiles outside a circle to avoid a boxy shape ---
                    if ((x * x + y * y) > (r * r))
                    {
                        continue; 
                    }

                    // --- CHANGED: Randomly skip some tiles for a more natural distribution ---
                    // e.g., 30% chance to skip
                    if (Random.value < 0.3f)
                    {
                        continue;
                    }

                    Vector3Int tilePos = centerTile + new Vector3Int(x, y, 0);

                    // Must be in validBiomeTiles (i.e. correct biome/tile type)
                    if (!validBiomeTiles.Contains(tilePos))
                        continue;

                    // Must match the rule's allowedTiles
                    TileBase tile = tilemap.GetTile(tilePos);
                    if (tile == null || !rule.allowedTiles.Contains(tile))
                        continue;

                    // Check spacing if required
                    if (rule.requireSpacing &&
                        IsNearOtherObject(tilePos, occupiedPositions, rule.cannotSpawnNear, rule.spacingDistance))
                    {
                        continue;
                    }

                    // If tile is free, spawn object
                    if (!occupiedPositions.ContainsKey(tilePos))
                    {
                        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);
                        GameObject spawnedObject = Instantiate(rule.objectToSpawn, worldPos, Quaternion.identity);

                        // Mark it occupied
                        ObjectType objTypeComponent = spawnedObject.GetComponent<ObjectType>();
                        occupiedPositions[tilePos] = (objTypeComponent != null) 
                            ? objTypeComponent.objectType 
                            : rule.objectType;

                        // Register with TileObjectManager
                        Vector2Int tileCoord = new Vector2Int(tilePos.x, tilePos.y);
                        ToolHit[] toolHit = spawnedObject.GetComponentsInChildren<ToolHit>();
                        foreach (ToolHit th in toolHit)
                        {
                            th.tileCoord = tileCoord;
                            TileObjectManager.Instance.RegisterObject(tileCoord, th);
                        }

                        // Adjust sorting
                        SpriteRenderer sr = spawnedObject.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null)
                            sr.sortingOrder = Mathf.RoundToInt(-worldPos.y * 100);

                        clusterObjects.Add(spawnedObject);
                    }
                }
            }
        }

        return clusterObjects;
    }


    bool IsNearOtherObject(
        Vector3Int position, 
        Dictionary<Vector3Int, string> occupiedPositions, 
        List<string> restrictedTypes, 
        int spacing)
    {
        for (int x = -spacing; x <= spacing; x++)
        {
            for (int y = -spacing; y <= spacing; y++)
            {
                if (x == 0 && y == 0) continue;
                Vector3Int checkPos = position + new Vector3Int(x, y, 0);

                if (occupiedPositions.TryGetValue(checkPos, out string foundType))
                {
                    if (restrictedTypes.Contains(foundType))
                        return true;
                }
            }
        }
        return false;
    }
}
