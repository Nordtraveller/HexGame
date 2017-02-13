﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator {

    public static Texture2D noiseSource;

    public static int GenerateElevation()
    {
        return Random.Range(0, 5);
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * HexagonMetrics.noiseScale, position.z * HexagonMetrics.noiseScale);
    }
}
