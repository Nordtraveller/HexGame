using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HexagonGrid : MonoBehaviour {

    public int meshCountX = 4;
    public int meshCountZ = 4;

    int cellCountX, cellCountZ;

    public HexagonCell cellPrefab;
    public Text cellTextPrefab;
    public Texture2D noiseSource;
    public HexagonGridPart gridPrefab;

    HexagonCell[] cells;
    HexagonGridPart[] grids;

    void Awake()
    {
        MapGenerator.noiseSource = noiseSource;

        cellCountX = meshCountX * HexagonMetrics.meshPartSizeX;
        cellCountZ = meshCountZ * HexagonMetrics.meshPartSizeZ;

        NewParts();
        NewCells();
    }

    void NewParts()
    {
        grids = new HexagonGridPart[cellCountX * cellCountZ];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                HexagonGridPart grid = grids[i++] = Instantiate(gridPrefab);
                grid.transform.SetParent(transform);
            }
        }
    }

    void NewCells()
    {
        cells = new HexagonCell[cellCountX * cellCountZ];
        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                NewCell(x, z, i++, MapGenerator.GenerateElevation());
            }
        }
    }

    void NewCell(int x, int z, int i, int elevation)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexagonMetrics.innerRadius * 2f);
        position.y = elevation * HexagonMetrics.elevationStep; 
        position.z = z * (HexagonMetrics.outerRadius * 1.5f);

        HexagonCell cell = cells[i] = Instantiate<HexagonCell>(cellPrefab);
        cell.transform.localPosition = position;
        cell.coordinates = HexagonCoordinates.FromOffsetCoordinates(x, z);
        cell.elevation = elevation;

        if (x > 0)
        {
            cell.SetNeighbor(HexagonDirections.W, cells[i - 1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexagonDirections.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexagonDirections.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexagonDirections.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexagonDirections.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text textCoordinates = Instantiate<Text>(cellTextPrefab);
        textCoordinates.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        textCoordinates.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = textCoordinates.rectTransform;

        Vector3 uiPosition = cell.uiRect.localPosition;
        uiPosition.z = elevation * -HexagonMetrics.elevationStep;
        cell.uiRect.localPosition = uiPosition;

        AddCellToMeshPart(x, z, cell);

    }

    void AddCellToMeshPart(int x, int z, HexagonCell cell)
    {
        int gridX = x / HexagonMetrics.meshPartSizeX;
        int gridZ = z / HexagonMetrics.meshPartSizeZ;
        HexagonGridPart grid = grids[gridX + gridZ * cellCountX];

        int localX = x - gridX * HexagonMetrics.meshPartSizeX;
        int localZ = z - gridZ * HexagonMetrics.meshPartSizeZ;
        grid.NewCell(localX + localZ * HexagonMetrics.meshPartSizeX, cell);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            TouchCell(hit.point);
        }
    }

    void TouchCell(Vector3 position)
    {
        HexagonCoordinates coordinates = HexagonCoordinates.FromPosition(position);
        position = transform.InverseTransformPoint(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        HexagonCell cell = cells[index];
        Debug.Log("touched at " + coordinates.ToString() + " elevation is " + cell.GetElevation()) ;
    }

}
