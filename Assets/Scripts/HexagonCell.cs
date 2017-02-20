using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonCell : MonoBehaviour {

    public RectTransform uiRect;
    public HexagonCoordinates coordinates;
    public HexagonGridPart gridPart;
    public TerrainType terraintype;

    public int elevation = int.MinValue;

    [SerializeField]
    HexagonCell[] neighbors;

    void Start()
    {
        if (gridPart)
        {
            gridPart.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexagonCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.gridPart != gridPart)
                {
                    neighbor.gridPart.Refresh();
                }
            }
        }
        SetTerrain();
    }

    public HexagonCell GetNeighbor(HexagonDirections direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexagonDirections direction, HexagonCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public int GetElevation()
    {
        return elevation;
    }

    public HexagonEdgeType GetEdgeType(HexagonDirections direction)
    {
        return HexagonMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    public HexagonEdgeType GetEdgeType(HexagonCell other)
    {
        return HexagonMetrics.GetEdgeType(elevation, other.elevation);
    }

    public void SetTerrain()
    {
       terraintype = MapGenerator.GenerateTerrainType();      
    }

    public TerrainType GetTerrain()
    {
        return terraintype;
    }

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }
}
