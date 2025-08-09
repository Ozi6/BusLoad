using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tunnel : MapObject
{
    public Vector2Int SpawnDirection { get; set; }
    public List<PassengerData> PassengerQueue { get; set; } = new List<PassengerData>();
    public int CurrentSpawnIndex { get; set; } = 0;
    [SerializeField] private TextMeshPro passengersInTunnelText;

    public bool HasPassengersLeft => CurrentSpawnIndex < PassengerQueue.Count;

    private void Start()
    {
        UpdatePassengerText();
    }

    public PassengerData PeekNextPassenger()
    {
        if (!HasPassengersLeft)
            return null;

        return PassengerQueue[CurrentSpawnIndex];
    }

    public void ConsumeNextPassenger()
    {
        if (HasPassengersLeft)
        {
            CurrentSpawnIndex++;
            UpdatePassengerText();
        }
    }

    public void ResetSpawnIndex()
    {
        CurrentSpawnIndex = 0;
        UpdatePassengerText();
    }

    private void UpdatePassengerText()
    {
        if (passengersInTunnelText != null)
            passengersInTunnelText.text = $"{PassengerQueue.Count - CurrentSpawnIndex}";
    }
}