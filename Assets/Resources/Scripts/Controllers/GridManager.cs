using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private Transform gridParent;
    private GameObject gridSquarePrefab;
    private Dictionary<Vector2Int, MapObject> gridObjects;
    private float gridSpacing;
    private int gridWidth;
    private int gridHeight;

    public GridManager(Transform parent, GameObject squarePrefab, Dictionary<Vector2Int, MapObject> objects,
                       int width, int height, float spacing)
    {
        gridParent = parent;
        gridSquarePrefab = squarePrefab;
        gridObjects = objects;
        gridWidth = width;
        gridHeight = height;
        gridSpacing = spacing;
    }

    public void InitializeGrid()
    {
        if (gridSquarePrefab == null) return;
        if (gridParent == null) gridParent = new GameObject("GridParent").transform;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    gridParent.position.x + x * gridSpacing,
                    0.1f,
                    gridParent.position.z + y * gridSpacing
                );
                GameObject gridSquare = Object.Instantiate(gridSquarePrefab, worldPos, Quaternion.identity, gridParent);
                gridSquare.name = $"GridSquare{x}_{y}";
                gridSquare.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            }
        }
    }

    public bool HasOccupantAt(Vector2Int position) => gridObjects.ContainsKey(position);
    public bool HasPassengerAt(Vector2Int position) => gridObjects.TryGetValue(position, out var obj) && obj is Passenger;

    public void RemoveOccupantFromGrid(Vector2Int position)
    {
        if (gridObjects.TryGetValue(position, out var obj))
        {
            gridObjects.Remove(position);
            TunnelManager.Instance.CheckForTunnelSpawns(position);
            FloodFillManager.Instance.FloodFillInteractable(position);
        }
    }
}
