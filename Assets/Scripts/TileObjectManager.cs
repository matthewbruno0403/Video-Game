using System.Collections.Generic;
using UnityEngine;

public class TileObjectManager : MonoBehaviour
{
    // Singleton pattern (optional) so you can easily access this from anywhere
    public static TileObjectManager Instance;

    // Maps tile coordinates → the breakable occupant on that tile
    private Dictionary<Vector2Int, ToolHit> occupantMap = new Dictionary<Vector2Int, ToolHit>();

    private void Awake()
    {
        // Simple singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Registers a breakable object (ToolHit) as occupying a specific tile.
    /// </summary>
    public void RegisterObject(Vector2Int tileCoord, ToolHit toolHit)
    {
        occupantMap[tileCoord] = toolHit;
    }

    /// <summary>
    /// Removes any occupant on tileCoord (e.g., after it's destroyed).
    /// </summary>
    public void UnregisterObject(Vector2Int tileCoord)
    {
        if (occupantMap.ContainsKey(tileCoord))
        {
            occupantMap.Remove(tileCoord);
        }
    }

    /// <summary>
    /// Returns the occupant (ToolHit) on the given tile, or null if none.
    /// </summary>
    public ToolHit GetOccupant(Vector2Int tileCoord)
    {
        occupantMap.TryGetValue(tileCoord, out ToolHit occupant);
        return occupant;
    }
}
