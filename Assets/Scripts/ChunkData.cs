using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    // Has this chunk been populated at least once?
    public bool hasBeenPopulated;

    // List of all objects spawned in this chunk (saved data only—no actual GameObjects).
    public List<SpawnedObjectData> spawnedObjects;

    public ChunkData()
    {
        hasBeenPopulated = false;
        spawnedObjects = new List<SpawnedObjectData>();
    }
}

// A simple record of one spawned object in the chunk:
[System.Serializable]
public class SpawnedObjectData
{
    // We’ll store the prefab by name or an ID you can match to an actual Prefab in ChunkManager
    public string prefabId;

    // Position/rotation so we can respawn the object in the exact same spot/orientation
    public Vector3 position;
    public Quaternion rotation;
}
