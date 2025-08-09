using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassengerData
{
    public PassengerColor color;
    public Vector2Int gridPosition;
    public List<string> traitTypes;
    public List<TraitConfiguration> traitConfigs = new List<TraitConfiguration>();
}