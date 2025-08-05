using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    public static PassengerController Instance;
    public Queue<Passenger> waitingQueue = new Queue<Passenger>();
    public int maxQueueSize = 5;
    private List<RopedTrait> ropedTraits = new List<RopedTrait>();

    private void Awake() => Instance = this;

    public void RegisterRoped(RopedTrait roped) => ropedTraits.Add(roped);
    public void UnregisterRoped(RopedTrait roped) => ropedTraits.Remove(roped);

    public void CheckRopedNearby(Vector2Int position)
    {
        foreach (RopedTrait roped in ropedTraits)
            roped.CheckUntie(position);
    }

    public void SelectPassenger(Passenger passenger)
    {
        CheckRopedNearby(passenger.GridPosition);
        Bus currentBus = BusController.Instance.CurrentBus;

        if (BusController.Instance.IsBusAtBoardingPoint && currentBus != null && passenger.CanBoardBus(currentBus))
        {
            currentBus.AddPassenger(passenger);
            GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);
        }
        else
        {
            if (QueueManager.Instance.IsQueueFull())
                QueueManager.Instance.RemoveOldestIfFull();
            QueueManager.Instance.AddToQueue(passenger);
            GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);
        }
    }

    public void FloodFillInteractable(Vector2Int startPos)
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);
        visited[startPos] = true;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

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
                if (next.x >= 0 && next.x < gridSize.x && next.y >= 0 && next.y < gridSize.y && !visited.ContainsKey(next))
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }
    }

    public void ProcessQueue()
    {
        if (!BusController.Instance.IsBusAtBoardingPoint || BusController.Instance.CurrentBus == null)
            return;
        Bus currentBus = BusController.Instance.CurrentBus;
        QueueManager.Instance.ProcessQueueForBus(currentBus);
    }
}