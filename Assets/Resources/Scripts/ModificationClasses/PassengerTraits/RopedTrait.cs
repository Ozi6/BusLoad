using System.Collections.Generic;
using UnityEngine;

public class RopedTrait : PassengerTrait
{
    private Passenger owner;
    private List<Vector2Int> initialNeighborPositions = new List<Vector2Int>();
    private int ropeCount = 2;

    private void Start()
    {
        owner = GetComponent<Passenger>();
        InitializeNeighborPositions();
        ropeCount = Mathf.Min(initialNeighborPositions.Count, 3);
        UpdateVisualIndicator();
    }

    private void InitializeNeighborPositions()
    {
        foreach (Vector2Int direction in DirectionVectors.CardinalDirections)
        {
            Vector2Int checkPos = owner.Position + direction;
            if (checkPos.x >= 0 && checkPos.x < GameManager.Instance.GetGridBounds().x &&
                checkPos.y >= 0 && checkPos.y < GameManager.Instance.GetGridBounds().y)
            {
                if (GameManager.Instance.gridObjects.ContainsKey(checkPos))
                    initialNeighborPositions.Add(checkPos);
            }
        }
    }

    public override void OnSelected(Passenger passenger) { }
    public override bool CanBoard(Passenger passenger, Bus bus) => false;
    public override bool CanMove(Passenger passenger, Bus bus) => false;

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null)
            return;
        CheckUntie(clickedPosition);
    }

    private void CheckUntie(Vector2Int clickedPosition)
    {
        if (Mathf.Abs(owner.Position.x - clickedPosition.x) +
            Mathf.Abs(owner.Position.y - clickedPosition.y) != 1)
            return;

        int missingNeighborCount = 0;
        foreach (Vector2Int pos in initialNeighborPositions)
            if (!GameManager.Instance.gridObjects.ContainsKey(pos))
                missingNeighborCount++;

        int newRopeCount = Mathf.Max(0, ropeCount - missingNeighborCount);
        if (newRopeCount != ropeCount)
        {
            ropeCount = newRopeCount;
            UpdateVisualIndicator();

            if (ropeCount <= 0)
                owner.RemoveTrait(this);
        }
    }

    public int GetRopeCount() => ropeCount;
}