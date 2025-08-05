
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AStarPathfinder
{
    private readonly int gridSize;
    private readonly GameManager gameManager;

    public AStarPathfinder(int gridSize, GameManager gameManager)
    {
        this.gridSize = gridSize;
        this.gameManager = gameManager;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var allNodes = new Dictionary<Vector2Int, Node>();

        var startNode = new Node(start, 0, GetHeuristic(start, goal), null);
        openSet.Add(startNode);
        allNodes[start] = startNode;

        while (openSet.Count > 0)
        {
            var currentNode = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            if (currentNode.Position == goal)
                return ReconstructPath(currentNode);

            foreach (var neighbor in GetNeighbors(currentNode.Position))
            {
                if (closedSet.Contains(neighbor) || !IsWalkable(neighbor))
                    continue;

                float tentativeGCost = currentNode.GCost + GetDistance(currentNode.Position, neighbor);

                if (!allNodes.ContainsKey(neighbor))
                {
                    var neighborNode = new Node(neighbor, tentativeGCost, GetHeuristic(neighbor, goal), currentNode);
                    allNodes[neighbor] = neighborNode;
                    openSet.Add(neighborNode);
                }
                else
                {
                    var neighborNode = allNodes[neighbor];
                    if (tentativeGCost < neighborNode.GCost)
                    {
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.Parent = currentNode;

                        if (!openSet.Contains(neighborNode))
                            openSet.Add(neighborNode);
                    }
                }
            }
        }

        return new List<Vector2Int>();
    }

    public Vector2Int FindHighestEmptySquare(Vector2Int fromPosition)
    {
        Vector2Int bestPosition = fromPosition;
        int highestY = fromPosition.y;
        float centerX = (gridSize - 1) / 2f;
        float bestDistanceToCenter = float.MaxValue;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = gridSize - 1; y >= 0; y--)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (!IsWalkable(position))
                    continue;

                var path = FindPath(fromPosition, position);
                if (path.Count == 0)
                    continue;

                float distanceToCenter = Mathf.Abs(x - centerX);

                if (y > highestY || (y == highestY && distanceToCenter < bestDistanceToCenter))
                {
                    highestY = y;
                    bestPosition = position;
                    bestDistanceToCenter = distanceToCenter;
                }
            }
        }

        return bestPosition;
    }

    public List<Vector2Int> FindPathToHighestEmptySquare(Vector2Int start)
    {
        Vector2Int highestEmpty = FindHighestEmptySquare(start);
        if (highestEmpty == start)
            return new List<Vector2Int> { start };

        return FindPath(start, highestEmpty);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>();

        foreach (var direction in DirectionVectors.CardinalDirections)
        {
            Vector2Int neighbor = position + direction;
            if (IsValidPosition(neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize &&
               position.y >= 0 && position.y < gridSize;
    }

    private bool IsWalkable(Vector2Int position)
    {
        if (!IsValidPosition(position))
            return false;

        return !gameManager.HasPassengerAt(position);
    }

    private float GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return GetDistance(a, b);
    }

    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        var path = new List<Vector2Int>();
        var currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private class Node
    {
        public Vector2Int Position { get; }
        public float GCost { get; set; }
        public float HCost { get; }
        public float FCost => GCost + HCost;
        public Node Parent { get; set; }

        public Node(Vector2Int position, float gCost, float hCost, Node parent)
        {
            Position = position;
            GCost = gCost;
            HCost = hCost;
            Parent = parent;
        }
    }
}