using UnityEngine;

public static class HexagonMetrics {

    public const int meshPartSizeX = 4, meshPartSizeZ = 4;

    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float elevationStep = 1f;
    public const int slopeNumber = 2;
    public const int slopeSteps = slopeNumber *2 + 1;
    public const float heightSlopeSize = 1.0f / (slopeNumber + 1);
    public const float distanceSlopeSize = 1.0f /slopeSteps;

    public const float solidFactor = 0.85f;
    public const float connectFactor = 1.0f - solidFactor;

    public const float perturbationStrength = 3f;
    public const float noiseScale = 0.003f;

    static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    public static Vector3 GetFirstCorner(HexagonDirections direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexagonDirections direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstVisibleCorner(HexagonDirections direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondVisibleCorner(HexagonDirections direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    public static Vector3 GetBridge(HexagonDirections direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * connectFactor;
    }

    public static Vector3 SlopePoint(Vector3 a, Vector3 b, int step)
    {
        float distance = step * distanceSlopeSize;
        float height = ((step + 1) / 2) * heightSlopeSize;
        a.x += (b.x - a.x) * distance;
        a.z += (b.z - a.z) * distance;
        a.y += (b.y - a.y) * height;
        return a;
    }

    public static HexagonEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexagonEdgeType.Flat;
        }
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexagonEdgeType.Slope;
        }
        return HexagonEdgeType.Cliff;
    }
}
