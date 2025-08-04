using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public PassengerColor Color { get; set; }
    public List<Passenger> Passengers { get; set; } = new List<Passenger>();
    public List<BusTrait> traits = new List<BusTrait>();

    public void AddPassenger(Passenger passenger)
    {
        if (!passenger.CanBoardBus(this) || Passengers.Count >= 3)
            return;

        Passengers.Add(passenger);
        passenger.transform.SetParent(transform);
        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);

        if (Passengers.Count >= 3)
            BusController.Instance.DepartBus();
    }
}
