using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    public Material gridMaterial;
    public Material highlightMaterial;

    [Header("Visual")]
    public GameObject gridCellPrefab;
    public Transform gridParent;

    private Dictionary<Vector2Int, GameObject> gridCells = new Dictionary<Vector2Int, GameObject>();
    private GameObject highlightedCell;

    public static GridManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        ClearGrid();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                Vector3 worldPos = GridToWorldPosition(gridPos);

                GameObject cell = CreateGridCell(worldPos, gridPos);
                gridCells[gridPos] = cell;
            }
        }
    }

    private GameObject CreateGridCell(Vector3 worldPos, Vector2Int gridPos)
    {
        GameObject cell;

        if (gridCellPrefab != null)
            cell = Instantiate(gridCellPrefab, gridParent);
        else
        {
            cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.transform.SetParent(gridParent);

            if (cell.GetComponent<Collider>())
                DestroyImmediate(cell.GetComponent<Collider>());
        }

        cell.name = $"GridCell_{gridPos.x}_{gridPos.y}";
        cell.transform.position = worldPos;
        cell.transform.rotation = Quaternion.Euler(90, 0, 0);
        cell.transform.localScale = Vector3.one * cellSize * 0.9f;

        GridCell gridCellComponent = cell.AddComponent<GridCell>();
        gridCellComponent.gridPosition = gridPos;

        Renderer renderer = cell.GetComponent<Renderer>();
        if (renderer && gridMaterial)
        {
            renderer.material = gridMaterial;
        }

        return cell;
    }

    public void ClearGrid()
    {
        foreach (var cell in gridCells.Values)
        {
            if (cell != null)
                DestroyImmediate(cell);
        }
        gridCells.Clear();
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.z / cellSize)
        );
    }

    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    public void HighlightCell(Vector2Int gridPos)
    {
        if (highlightedCell != null)
        {
            Renderer renderer = highlightedCell.GetComponent<Renderer>();
            if (renderer && gridMaterial)
                renderer.material = gridMaterial;
        }

        if (gridCells.TryGetValue(gridPos, out GameObject cell))
        {
            highlightedCell = cell;
            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer && highlightMaterial)
                renderer.material = highlightMaterial;
        }
    }

    public void ClearHighlight()
    {
        if (highlightedCell != null)
        {
            Renderer renderer = highlightedCell.GetComponent<Renderer>();
            if (renderer && gridMaterial)
                renderer.material = gridMaterial;
            highlightedCell = null;
        }
    }
}