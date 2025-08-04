using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public PassengerColor Color { get; set; }
    public Vector2Int GridPosition { get; set; }
    public List<PassengerTrait> traits = new List<PassengerTrait>();

    private void OnMouseDown()
    {
        foreach (PassengerTrait trait in traits)
            trait.OnSelected(this);

        PassengerController.Instance.SelectPassenger(this);
    }

    public bool CanBoardBus(Bus bus)
    {
        if (Color != bus.Color) return false;

        foreach (PassengerTrait trait in traits)
            if (!trait.CanBoard(this, bus)) return false;

        foreach (BusTrait trait in bus.traits)
            if (!trait.CanAcceptPassenger(bus, this)) return false;

        return true;
    }
}