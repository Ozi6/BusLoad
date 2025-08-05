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

        if (passenger.CanBoardBus(currentBus))
            currentBus.AddPassenger(passenger);
        else
        {
            if (QueueManager.Instance.IsQueueFull())
                QueueManager.Instance.RemoveOldestIfFull();

            QueueManager.Instance.AddToQueue(passenger);
        }
    }

    public void ProcessQueue()
    {
        Bus currentBus = BusController.Instance.CurrentBus;
        QueueManager.Instance.ProcessQueueForBus(currentBus);
    }
}