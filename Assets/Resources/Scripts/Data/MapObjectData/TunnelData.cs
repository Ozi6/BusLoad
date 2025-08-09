using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TunnelData
{
    public Vector2Int gridPosition;
    public Vector2Int direction;
    public List<PassengerData> passengerQueue = new List<PassengerData>();
}