using UnityEngine;

public static class DirectionVectors
{
    public static readonly Vector2Int[] CardinalDirections = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0)   // Left
    };
}