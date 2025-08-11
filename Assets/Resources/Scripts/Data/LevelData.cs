using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName = "";
    [Header("Grid Settings")]
    public int gridWidth = 12;
    public int gridHeight = 12;
    public float gridSpacing = 2f;

    [Header("Level Objects")]
    public List<PassengerData> passengers = new List<PassengerData>();
    public List<WallData> walls = new List<WallData>();
    public List<TunnelData> tunnels = new List<TunnelData>();
    public List<BusData> buses = new List<BusData>();
}