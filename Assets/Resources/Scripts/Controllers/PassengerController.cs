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
            if (waitingQueue.Count >= maxQueueSize)
                waitingQueue.Dequeue();
            waitingQueue.Enqueue(passenger);
        }
    }

    public void ProcessQueue()
    {
        Bus currentBus = BusController.Instance.CurrentBus;
        Queue<Passenger> tempQueue = new Queue<Passenger>();

        while (waitingQueue.Count > 0)
        {
            Passenger passenger = waitingQueue.Dequeue();
            if (passenger.CanBoardBus(currentBus))
            {
                currentBus.AddPassenger(passenger);
                if (currentBus.Passengers.Count >= 3) break;
            }
            else
            {
                tempQueue.Enqueue(passenger);
            }
        }

        waitingQueue = tempQueue;
    }
}