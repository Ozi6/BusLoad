using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MapObject
{
    public Vector2Int SpawnDirection { get; set; }
    public List<PassengerData> PassengerQueue { get; set; } = new List<PassengerData>();
    public int CurrentSpawnIndex { get; set; } = 0;

    public bool HasPassengersLeft => CurrentSpawnIndex < PassengerQueue.Count;

    public PassengerData PeekNextPassenger()
    {
        if (!HasPassengersLeft)
            return null;

        return PassengerQueue[CurrentSpawnIndex];
    }

    public void ConsumeNextPassenger()
    {
        if (HasPassengersLeft)
            CurrentSpawnIndex++;
    }

    public PassengerData GetNextPassenger()
    {
        if (!HasPassengersLeft)
            return null;

        PassengerData passenger = PassengerQueue[CurrentSpawnIndex];
        CurrentSpawnIndex++;
        return passenger;
    }

    public void ResetSpawnIndex()
    {
        CurrentSpawnIndex = 0;
    }
}