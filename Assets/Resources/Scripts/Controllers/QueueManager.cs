using UnityEngine;
using System.Collections.Generic;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance;
    public Transform[] queuePositions = new Transform[5];
    private Passenger[] queuedPassengers = new Passenger[5];

    private void Awake()
    {
        Instance = this;
    }

    public bool AddToQueue(Passenger passenger)
    {
        int emptySlot = GetLeftmostEmptySlot();
        if (emptySlot == -1)
            return false;

        queuedPassengers[emptySlot] = passenger;
        MovementManager.Instance.MoveGradual(passenger.gameObject, queuePositions[emptySlot]);

        return true;
    }

    public void ProcessQueueForBus(Bus bus)
    {
        List<int> toRemove = new List<int>();

        for (int i = 0; i < queuedPassengers.Length; i++)
        {
            if (queuedPassengers[i] != null && queuedPassengers[i].CanBoardBus(bus))
            {
                Passenger passenger = queuedPassengers[i];
                queuedPassengers[i] = null;
                toRemove.Add(i);

                MovementManager.Instance.MoveGradual(passenger.gameObject, bus.transform, 0f, () => {
                    bus.AddPassenger(passenger);
                });

                if (bus.Passengers.Count >= 3) break;
            }
        }

        if (toRemove.Count > 0)
            CompactQueue();
    }

    private void CompactQueue()
    {
        List<Passenger> remaining = new List<Passenger>();

        for (int i = 0; i < queuedPassengers.Length; i++)
        {
            if (queuedPassengers[i] != null)
                remaining.Add(queuedPassengers[i]);
            queuedPassengers[i] = null;
        }

        for (int i = 0; i < remaining.Count; i++)
        {
            queuedPassengers[i] = remaining[i];
            MovementManager.Instance.MoveGradual(remaining[i].gameObject, queuePositions[i]);
        }
    }

    private int GetLeftmostEmptySlot()
    {
        for (int i = 0; i < queuedPassengers.Length; i++)
        {
            if (queuedPassengers[i] == null)
                return i;
        }
        return -1;
    }

    public bool IsQueueFull()
    {
        return GetLeftmostEmptySlot() == -1;
    }

    public void RemoveOldestIfFull()
    {
        if (!IsQueueFull())
            return;
        queuedPassengers[0] = null;
        CompactQueue();
    }
}