using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonGridPart : MonoBehaviour {

    HexagonCell[] cells;

    HexagonMesh hexagonMesh;
    Canvas gridCanvas;

    void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexagonMesh = GetComponentInChildren<HexagonMesh>();

        cells = new HexagonCell[HexagonMetrics.meshPartSizeX * HexagonMetrics.meshPartSizeZ];
    }

    public void Refresh()
    {
        hexagonMesh.Triangulate(cells);
    }

    public void NewCell(int index, HexagonCell cell)
    {
        cells[index] = cell;
        cell.gridPart = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }
}
