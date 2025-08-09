using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public List<PassengerData> passengers = new List<PassengerData>();
    public List<WallData> walls = new List<WallData>();
    public List<TunnelData> tunnels = new List<TunnelData>();
    public List<BusData> buses = new List<BusData>();
}