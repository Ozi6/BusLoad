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
        if (IsPassengerInQueue(passenger))
            return true;

        int emptySlot = GetLeftmostEmptySlot();
        if (emptySlot == -1)
            return false;

        queuedPassengers[emptySlot] = passenger;
        if (IsQueueFull())
            GameOverManager.Instance.CheckGameOverConditions();
        return true;
    }

    public void MovePassengerToQueuePosition(Passenger passenger)
    {
        int slot = GetPassengerSlot(passenger);
        if (slot != -1)
            MovementManager.Instance.MoveGradual(passenger.gameObject, queuePositions[slot]);
    }

    public bool IsPassengerInQueue(Passenger passenger)
    {
        for (int i = 0; i < queuedPassengers.Length; i++)
            if (queuedPassengers[i] == passenger)
                return true;
        return false;
    }

    public void RemoveFromQueue(Passenger passenger)
    {
        for (int i = 0; i < queuedPassengers.Length; i++)
        {
            if (queuedPassengers[i] == passenger)
            {
                queuedPassengers[i] = null;
                if (MovementManager.Instance.HasActiveMovement(passenger.gameObject))
                    MovementManager.Instance.CancelMovement(passenger.gameObject);
                CompactQueue();
                return;
            }
        }
    }

    private int GetPassengerSlot(Passenger passenger)
    {
        for (int i = 0; i < queuedPassengers.Length; i++)
            if (queuedPassengers[i] == passenger)
                return i;
        return -1;
    }

    public void ProcessQueueForBus(Bus bus)
    {
        List<int> toRemove = new List<int>();
        int processedCount = 0;

        for (int i = 0; i < queuedPassengers.Length && processedCount < 3; i++)
        {
            if (queuedPassengers[i] != null && queuedPassengers[i].CanBoardBus(bus))
            {
                if (!bus.TryReserveSpot(queuedPassengers[i]))
                    break;
                Passenger passenger = queuedPassengers[i];
                queuedPassengers[i] = null;
                toRemove.Add(i);
                processedCount++;
                MovementManager.Instance.MoveGradual(passenger.gameObject, bus.transform, 0f, () => {
                    bus.AddPassenger(passenger);
                });
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
            if (queuedPassengers[i] == null)
                return i;
        return -1;
    }

    public bool IsQueueFull()
    {
        return GetLeftmostEmptySlot() == -1;
    }

    public void EmptyQueue()
    {
        List<Passenger> passengersToDestroy = new List<Passenger>();
        for (int i = 0; i < queuedPassengers.Length; i++)
        {
            if (queuedPassengers[i] != null)
            {
                if (MovementManager.Instance.HasActiveMovement(queuedPassengers[i].gameObject))
                    MovementManager.Instance.CancelMovement(queuedPassengers[i].gameObject);
                passengersToDestroy.Add(queuedPassengers[i]);
                queuedPassengers[i] = null;
            }
        }
        foreach (Passenger passenger in passengersToDestroy)
            if (passenger != null)
                Destroy(passenger.gameObject);
    }
}