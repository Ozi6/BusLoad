using UnityEngine;

public class RopedTrait : PassengerTrait
{
    private Passenger owner;

    public void Initialize(Passenger passenger)
    {
        owner = passenger;
        PassengerController.Instance.RegisterRoped(this);
    }

    public override void OnSelected(Passenger passenger) { }
    public override bool CanBoard(Passenger passenger, Bus bus) => false;

    public void CheckUntie(Vector2Int clickedPosition)
    {
        if (Vector2Int.Distance(owner.GridPosition, clickedPosition) > 2f) return;

        int nearbyCount = 0;
        Vector2Int[] neighbors = {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };

        foreach (Vector2Int offset in neighbors)
        {
            Vector2Int checkPos = owner.GridPosition + offset;
            if (GameManager.Instance.HasPassengerAt(checkPos))
                nearbyCount++;
        }

        if (nearbyCount >= 2)
        {
            PassengerController.Instance.UnregisterRoped(this);
            owner.traits.Remove(this);
            Destroy(this);
        }
    }
}