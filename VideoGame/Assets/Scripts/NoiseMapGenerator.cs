using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseMapGenerator 
{

    public static float GenerateNoiseAtPoint (float x, float y, float scale )
    {
        if (scale <= 0) {
            scale = 0.0001f;
        }

        float sampleX = x / scale;
        float sampleY = y / scale;

        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

        return perlinValue;
    }
}
