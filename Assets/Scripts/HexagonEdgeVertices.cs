using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HexagonEdgeVertices{

    public Vector3 v1, v2, v3, v4;

    public HexagonEdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
        v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
        v4 = corner2;
    }

    public static HexagonEdgeVertices SlopeLerp(HexagonEdgeVertices a, HexagonEdgeVertices b, int step)
    {
        HexagonEdgeVertices result;
        result.v1 = HexagonMetrics.SlopePoint(a.v1, b.v1, step);
        result.v2 = HexagonMetrics.SlopePoint(a.v2, b.v2, step);
        result.v3 = HexagonMetrics.SlopePoint(a.v3, b.v3, step);
        result.v4 = HexagonMetrics.SlopePoint(a.v4, b.v4, step);
        return result;
    }
}
