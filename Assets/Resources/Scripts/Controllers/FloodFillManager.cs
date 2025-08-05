using System.Collections.Generic;
using UnityEngine;

public class FloodFillManager : MonoBehaviour
{
    public static FloodFillManager Instance;

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0)   // Left
    };

    private void Awake() => Instance = this;

    public void FloodFillInteractable(Vector2Int startPos)
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPos);
        visited[startPos] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (GameManager.Instance.HasPassengerAt(current))
            {
                Passenger passenger = GameManager.Instance.gridPassengers[current];
                passenger.SetInteractable(true);
                continue;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next, gridSize) && !visited.ContainsKey(next))
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }
    }

    public void InitializeInteractablePassengers()
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        HashSet<Vector2Int> globallyReached = new HashSet<Vector2Int>();

        List<Vector2Int> highestEmptyPositions = FindHighestEmptyPositions();

        foreach (Vector2Int startPos in highestEmptyPositions)
        {
            if (globallyReached.Contains(startPos))
                continue;
            HashSet<Vector2Int> reachedInThisFlood = PerformInitializationFloodFill(startPos);
            foreach (Vector2Int pos in reachedInThisFlood)
                globallyReached.Add(pos);
        }
    }

    private List<Vector2Int> FindHighestEmptyPositions()
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        List<Vector2Int> highestEmpty = new List<Vector2Int>();

        int maxY = gridSize.y - 1;

        for (int x = 0; x < gridSize.x; x++)
        {
            Vector2Int pos = new Vector2Int(x, maxY);
            if (!GameManager.Instance.HasPassengerAt(pos))
                highestEmpty.Add(pos);
        }

        return highestEmpty;
    }

    private HashSet<Vector2Int> PerformInitializationFloodFill(Vector2Int startPos)
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (GameManager.Instance.HasPassengerAt(current))
            {
                Passenger passenger = GameManager.Instance.gridPassengers[current];
                passenger.SetInteractable(true);
                continue;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next, gridSize) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    private bool IsValidPosition(Vector2Int pos, Vector2Int gridSize)
    {
        return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
    }
}