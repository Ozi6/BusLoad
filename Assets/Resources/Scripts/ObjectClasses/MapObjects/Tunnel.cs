using UnityEngine;

public class Tunnel : MapObject
{
    public Vector2Int SpawnDirection { get; set; }
    public PassengerData SpawnTemplate { get; set; }
}