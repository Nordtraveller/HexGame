using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexagonMesh : MonoBehaviour
{

    Mesh hexagonMesh;
    MeshCollider meshCollider;
    static List<Vector3> vertices = new List<Vector3>();
    static List<Vector2> uv = new List<Vector2>();
    List<int> triangles = new List<int>();

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexagonMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexagonMesh.name = "Hexagon Mesh";
    }

    public void Triangulate(HexagonCell[] cells)
    {
        hexagonMesh.Clear();
        vertices.Clear();
        uv.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        hexagonMesh.vertices = vertices.ToArray();
        hexagonMesh.uv = uv.ToArray();
        hexagonMesh.triangles = triangles.ToArray();
        hexagonMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexagonMesh;
    }

    void Triangulate(HexagonCell cell)
    {
        for (HexagonDirections d = HexagonDirections.NE; d <= HexagonDirections.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexagonDirections direction, HexagonCell cell)
    {
        Vector3 center = cell.Position;
        HexagonEdgeVertices e = new HexagonEdgeVertices(
             center + HexagonMetrics.GetFirstVisibleCorner(direction),
             center + HexagonMetrics.GetSecondVisibleCorner(direction)
         );

        TriangulateEdgeFan(center, e);


        if (direction <= HexagonDirections.SE)
        {
            TriangulateConnection(direction, cell, e);
        }
    }

    void TriangulateConnection(HexagonDirections direction, HexagonCell cell, HexagonEdgeVertices e1)
    {
        HexagonCell neighbor = cell.GetNeighbor(direction);

        Vector3 bridge = HexagonMetrics.GetBridge(direction);
        if (neighbor == null)
        {
            return;
        }
        bridge.y = neighbor.Position.y - cell.Position.y;
        HexagonEdgeVertices e2 = new HexagonEdgeVertices(e1.v1 + bridge, e1.v4 + bridge);

        if (cell.GetEdgeType(direction) == HexagonEdgeType.Slope)
        {
            TriangulateSlope(e1, e2);
        }
        else
        {
            TriangulateEdgeStrip(e1, e2);
        }

        HexagonCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexagonDirections.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v4 + HexagonMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;
            if (cell.elevation <= neighbor.elevation)
            {
                if (cell.elevation <= nextNeighbor.elevation)
                {
                    TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                }
			}
			else if (neighbor.elevation <= nextNeighbor.elevation)
            {
				TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, e1.v4, cell);
			}
			else
            {
				TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
			}
        }

    }

    void TriangulateSlope(HexagonEdgeVertices begin, HexagonEdgeVertices end)
    {
        HexagonEdgeVertices e = HexagonEdgeVertices.SlopeLerp(begin, end, 1);
        TriangulateEdgeStrip(begin, e);

        for(int i = 2; i<HexagonMetrics.slopeSteps; i++)
        {
            HexagonEdgeVertices eOld = e;
            e = HexagonEdgeVertices.SlopeLerp(begin, end, i);
            TriangulateEdgeStrip(eOld, e);
        }

        TriangulateEdgeStrip(e, end);
    }

    void TriangulateCorner(Vector3 bottom, HexagonCell bottomCell, Vector3 left, HexagonCell leftCell, Vector3 right, HexagonCell rightCell)
    {
        HexagonEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexagonEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexagonEdgeType.Slope)
        {
            if (rightEdgeType == HexagonEdgeType.Slope)
            {
                TriangulateCornerSlopes(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexagonEdgeType.Flat)
            {
                TriangulateCornerSlopes(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                TriangulateCornerSlopesAndCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (rightEdgeType == HexagonEdgeType.Slope)
        {
            if (leftEdgeType == HexagonEdgeType.Flat)
            {
                TriangulateCornerSlopes(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerCliffAndSlopes(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexagonEdgeType.Slope)
        {
            if (leftCell.elevation < rightCell.elevation)
            {
                TriangulateCornerCliffAndSlopes(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerSlopesAndCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else
        {
            AddTriangle(bottom, left, right);
        }
    }

    void TriangulateCornerSlopes(Vector3 begin, HexagonCell beginCell, Vector3 left, HexagonCell leftCell, Vector3 right, HexagonCell rightCell)
    {
        Vector3 v1 = HexagonMetrics.SlopePoint(begin, left, 1);
        Vector3 v2 = HexagonMetrics.SlopePoint(begin, right, 1);
        AddTriangle(begin, v1, v2);

        for (int i = 2; i < HexagonMetrics.slopeSteps; i++)
        {
            Vector3 v1Old = v1;
            Vector3 v2Old = v2;
            v1 = HexagonMetrics.SlopePoint(begin, left, i);
            v2 = HexagonMetrics.SlopePoint(begin, right, i);
            AddQuad(v1Old, v2Old, v1, v2);
        }

        AddQuad(v1, v2, left, right);
    }

    void TriangulateCornerSlopesAndCliff(Vector3 begin, HexagonCell beginCell, Vector3 left, HexagonCell leftCell, Vector3 right, HexagonCell rightCell)
    {
        float b = 1f / (rightCell.elevation - beginCell.elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(Perturbations(begin), Perturbations(right), b);

        TriangulateCornerCliff(begin, beginCell, left, leftCell, boundary);

        if (leftCell.GetEdgeType(rightCell) == HexagonEdgeType.Slope)
        {
            TriangulateCornerCliff(left, leftCell, right, rightCell, boundary);
        }
        else
        {
            AddTriangleWithoutPerturabation(Perturbations(left), Perturbations(right), boundary);
        }
    }

    void TriangulateCornerCliffAndSlopes(Vector3 begin, HexagonCell beginCell, Vector3 left, HexagonCell leftCell, Vector3 right, HexagonCell rightCell)
    {
        float b = 1f / (leftCell.elevation - beginCell.elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(Perturbations(begin), Perturbations(left), b);

        TriangulateCornerCliff(right, rightCell, begin, beginCell, boundary);

        if (leftCell.GetEdgeType(rightCell) == HexagonEdgeType.Slope)
        {
            TriangulateCornerCliff(left, leftCell, right, rightCell, boundary);
        }
        else
        {
            AddTriangleWithoutPerturabation(Perturbations(left), Perturbations(right), boundary);
        }
    }

    void TriangulateCornerCliff(Vector3 begin, HexagonCell beginCell, Vector3 left, HexagonCell leftCell, Vector3 boundary)
    {
        Vector3 v2 = HexagonMetrics.SlopePoint(begin, left, 1);

        AddTriangleWithoutPerturabation(Perturbations(begin), Perturbations(v2), boundary);

        for (int i = 2; i < HexagonMetrics.slopeSteps; i++)
        {
            Vector3 v1 = v2;
            v2 = HexagonMetrics.SlopePoint(begin, left, i);
            AddTriangleWithoutPerturabation(Perturbations(v1), Perturbations(v2), boundary);
        }
        AddTriangleWithoutPerturabation(Perturbations(v2), Perturbations(left), boundary);
    }

    void TriangulateEdgeFan(Vector3 center, HexagonEdgeVertices edge)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangle(center, edge.v3, edge.v4);
    }

    void TriangulateEdgeStrip(HexagonEdgeVertices e1, HexagonEdgeVertices e2)
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturbations(v1));
        vertices.Add(Perturbations(v2));
        vertices.Add(Perturbations(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleWithoutPerturabation(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturbations(v1));
        vertices.Add(Perturbations(v2));
        vertices.Add(Perturbations(v3));
        vertices.Add(Perturbations(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    Vector3 Perturbations(Vector3 position)
    {
        Vector3 sample = MapGenerator.SampleNoise(position);
        position.x += (sample.x * 2f - 1f ) * HexagonMetrics.perturbationStrength;
        //position.y += (sample.y * 2f - 1f ) * (HexagonMetrics.perturbationStrength/20);
        position.z += (sample.z * 2f - 1f ) * HexagonMetrics.perturbationStrength;
        return position;
    }

}