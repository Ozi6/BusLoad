using System.Collections.Generic;

[System.Serializable]
public class BusData
{
    public PassengerColor color;
    public List<string> traitTypes;
    public List<TraitConfiguration> traitConfigs = new List<TraitConfiguration>();
}