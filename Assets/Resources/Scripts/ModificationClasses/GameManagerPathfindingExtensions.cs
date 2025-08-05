using System.Collections.Generic;
using UnityEngine;

public static class GameManagerPathfindingExtensions
{
    private static AStarPathfinder pathfinder;

    public static AStarPathfinder GetPathfinder(this GameManager gameManager)
    {
        if (pathfinder == null)
        {
            Vector2Int bounds = gameManager.GetGridBounds();
            pathfinder = new AStarPathfinder(bounds.x, gameManager);
        }
        return pathfinder;
    }

    public static List<Vector2Int> FindPathToHighestEmpty(this GameManager gameManager, Vector2Int startPosition)
    {
        return gameManager.GetPathfinder().FindPathToHighestEmptySquare(startPosition);
    }

    public static Vector2Int FindHighestEmptyPosition(this GameManager gameManager, Vector2Int fromPosition)
    {
        return gameManager.GetPathfinder().FindHighestEmptySquare(fromPosition);
    }
}