using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int gridSize = 50;
    public float scale = 10f;
    public float minHeight = 0f;
    public float maxHeight = 10f;

    public GameObject waterPrefab;
    public GameObject sandPrefab;
    public GameObject dirtPrefab;
    public GameObject grassPrefab;

    private void Start()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                float noiseValue = GenerateNoiseAtPoint(x, z);
                Vector3 position = new Vector3(x, noiseValue * (maxHeight - minHeight) + minHeight, z);
                GameObject terrainPrefab = GetTerrainPrefab(noiseValue);
                Instantiate(terrainPrefab, position, Quaternion.identity);
            }
        }
    }

    private float GenerateNoiseAtPoint(int x, int z)
    {
        float sampleX = (float)x / gridSize * scale;
        float sampleZ = (float)z / gridSize * scale;
        return Mathf.PerlinNoise(sampleX, sampleZ);
    }

    private GameObject GetTerrainPrefab(float noiseValue)
    {
        if (noiseValue < 0.25f)
        {
            return waterPrefab;
        }
        else if (noiseValue < 0.5f)
        {
            return sandPrefab;
        }
        else if (noiseValue < 0.75f)
        {
            return dirtPrefab;
        }
        else
        {
            return grassPrefab;
        }
    }
}