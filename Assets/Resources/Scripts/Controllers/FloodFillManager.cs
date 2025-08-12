using System.Collections.Generic;
using UnityEngine;

public class FloodFillManager : MonoBehaviour
{
    public static FloodFillManager Instance;

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
            if (GameManager.Instance.gridManager.HasOccupantAt(current))
            {
                MapObject obj = GameManager.Instance.gridObjects[current];
                obj.OnReachedByFlood();
                if (obj.BlocksFlood) continue;
            }
            foreach (Vector2Int dir in DirectionVectors.CardinalDirections)
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
        List<Vector2Int> startingPositions = FindHighestStartingPositions();

        foreach (Vector2Int startPos in startingPositions)
        {
            if (globallyReached.Contains(startPos)) continue;
            HashSet<Vector2Int> reachedInThisFlood = PerformInitializationFloodFill(startPos);
            foreach (Vector2Int pos in reachedInThisFlood)
                globallyReached.Add(pos);
        }
    }

    private List<Vector2Int> FindHighestStartingPositions()
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        List<Vector2Int> startingPositions = new List<Vector2Int>();
        int maxY = gridSize.y - 1;

        for (int x = 0; x < gridSize.x; x++)
        {
            Vector2Int pos = new Vector2Int(x, maxY);
            if (!GameManager.Instance.gridManager.HasOccupantAt(pos))
                startingPositions.Add(pos);
            else
            {
                MapObject obj = GameManager.Instance.gridObjects[pos];
                if (obj is Passenger)
                    startingPositions.Add(pos);
            }
        }

        return startingPositions;
    }

    private HashSet<Vector2Int> PerformInitializationFloodFill(Vector2Int startPos)
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);
        visited.Add(startPos);

        if (GameManager.Instance.gridManager.HasOccupantAt(startPos))
        {
            MapObject obj = GameManager.Instance.gridObjects[startPos];
            obj.OnReachedByFlood();
            if (obj is Passenger passenger)
                PassengerEvents.TriggerPassengerReachedByFlood(passenger);
            if (obj.BlocksFlood) return visited;
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (GameManager.Instance.gridManager.HasOccupantAt(current))
            {
                MapObject obj = GameManager.Instance.gridObjects[current];
                obj.OnReachedByFlood();
                if (obj is Passenger passenger)
                {
                    Debug.Log($"Passenger at {current} made interactable.");
                    PassengerEvents.TriggerPassengerReachedByFlood(passenger);
                }
                if (obj.BlocksFlood) continue;
            }
            foreach (Vector2Int dir in DirectionVectors.CardinalDirections)
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