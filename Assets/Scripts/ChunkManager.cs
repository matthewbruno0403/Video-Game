using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int viewDistanceInChunks = 2;

    [Header("References")]
    public Tilemap tilemap;            // The tilemap you drew by hand
    public ObjectSpawner objectSpawner; 
    public Transform player;            // So we know which chunks to load/unload around

    // Stores the “saved” state of each chunk (whether it’s been populated, which objects it has, etc.)
    private Dictionary<Vector2Int, ChunkData> chunkDataMap = new Dictionary<Vector2Int, ChunkData>();

    // Tracks the actual GameObjects currently active in each loaded chunk
    public Dictionary<Vector2Int, List<GameObject>> loadedChunkObjects = new Dictionary<Vector2Int, List<GameObject>>();

    // A simple registry for mapping prefab names → actual Prefabs
    [Header("Prefab Registry")]
    public List<GameObject> prefabRegistry;
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Build a quick lookup from prefab name to prefab GameObject
        foreach (var prefab in prefabRegistry)
        {
            if (prefab != null)
            {
                // For example, key by prefab.name
                prefabDict[prefab.name] = prefab;
            }
        }
    }

    void Update()
    {
        if (!player) return;

        // Which chunk is the player standing in?
        Vector2Int playerChunkCoord = WorldToChunkCoord(player.position);

        // Load all chunks in range
        for (int x = playerChunkCoord.x - viewDistanceInChunks; x <= playerChunkCoord.x + viewDistanceInChunks; x++)
        {
            for (int y = playerChunkCoord.y - viewDistanceInChunks; y <= playerChunkCoord.y + viewDistanceInChunks; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);

                // If we haven't loaded this chunk yet, load it
                if (!loadedChunkObjects.ContainsKey(chunkCoord))
                {
                    LoadChunk(chunkCoord);
                }
            }
        }

        // Unload any chunks that are out of range
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var c in loadedChunkObjects.Keys)
        {
            int distX = Mathf.Abs(c.x - playerChunkCoord.x);
            int distY = Mathf.Abs(c.y - playerChunkCoord.y);
            if (distX > viewDistanceInChunks || distY > viewDistanceInChunks)
            {
                chunksToUnload.Add(c);
            }
        }
        foreach (var chunkCoord in chunksToUnload)
        {
            UnloadChunk(chunkCoord);
        }
    }

    /// <summary>
    /// Converts a world position → tile coords → chunk coords.
    /// Marked public in case other scripts want to call it.
    /// </summary>
    public Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        int chunkX = Mathf.FloorToInt(tilePos.x / (float)chunkSize);
        int chunkY = Mathf.FloorToInt(tilePos.y / (float)chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    /// <summary>
    /// Loads (instantiates) all objects for the given chunkCoord.
    /// </summary>
    private void LoadChunk(Vector2Int chunkCoord)
    {
        // Mark that we've loaded it with an empty list for now
        loadedChunkObjects[chunkCoord] = new List<GameObject>();

        // Make sure we have ChunkData for this chunk
        if (!chunkDataMap.TryGetValue(chunkCoord, out ChunkData cData))
        {
            // If not found, create a brand-new chunk record
            cData = new ChunkData();
            chunkDataMap[chunkCoord] = cData;
        }

        // If it’s never been populated, call ObjectSpawner once to spawn initial objects
        if (!cData.hasBeenPopulated)
        {
            Vector3Int chunkOrigin = new Vector3Int(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0);
            BoundsInt chunkBounds = new BoundsInt(chunkOrigin, new Vector3Int(chunkSize, chunkSize, 1));

            // Spawn objects in that chunk’s region. 
            // (Make sure your ObjectSpawner has a SpawnObjectsInBounds method!)
            List<GameObject> newObjects = objectSpawner.SpawnObjectsInBounds(chunkBounds);

            // Convert those spawned objects into data so we can re-instantiate later
            foreach (var obj in newObjects)
            {
                if (obj == null) continue;

                // Record the basic info
                SpawnedObjectData soData = new SpawnedObjectData
                {
                    // Just store prefab name as an ID
                    prefabId = obj.name.Replace("(Clone)", "").Trim(),
                    position = obj.transform.position,
                    rotation = obj.transform.rotation
                };

                cData.spawnedObjects.Add(soData);
            }

            cData.hasBeenPopulated = true;
        }

        // Now instantiate objects from our saved chunk data
        foreach (var soData in cData.spawnedObjects)
        {
            // Look up the appropriate prefab by name
            if (prefabDict.TryGetValue(soData.prefabId, out GameObject prefab))
            {
                // Instantiate it
                GameObject go = Instantiate(prefab, soData.position, soData.rotation);
                // Keep track of it so we can destroy it on unload
                loadedChunkObjects[chunkCoord].Add(go);
            }
        }
    }

    /// <summary>
    /// Unloads (destroys) all GameObjects in a chunk but does NOT delete the chunk data.
    /// That way if the player returns, we can re-instantiate them in the same state.
    /// </summary>
    private void UnloadChunk(Vector2Int chunkCoord)
    {
        if (loadedChunkObjects.TryGetValue(chunkCoord, out List<GameObject> gos))
        {
            foreach (var go in gos)
            {
                if (go != null)
                {
                    // If you had to update chunkData with the object's final state,
                    // you'd do that here before destroying, e.g. if it had changed position.
                    Destroy(go);
                }
            }
        }
        loadedChunkObjects.Remove(chunkCoord);
        // The chunkDataMap remains in memory so we can re-instantiate next time.
    }
    
    public int GetLoadedChunkCount()
    {
        return loadedChunkObjects.Count;
    }
}
