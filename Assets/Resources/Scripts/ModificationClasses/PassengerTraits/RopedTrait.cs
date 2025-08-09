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
        Vector2Int[] neighbors = {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };
        foreach (Vector2Int offset in neighbors)
        {
            Vector2Int checkPos = owner.Position + offset;
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

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null)
            return;
        CheckUntie(clickedPosition);
    }

    private void CheckUntie(Vector2Int clickedPosition)
    {
        if (Vector2Int.Distance(owner.Position, clickedPosition) > 2f)
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