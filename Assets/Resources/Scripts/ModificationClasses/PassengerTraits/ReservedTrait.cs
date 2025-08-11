using UnityEngine;

public class ReservedTrait : PassengerTrait
{
    private Passenger owner;
    [SerializeField] private bool isReserved = true;

    private void Start()
    {
        owner = GetComponent<Passenger>();
    }

    public override void OnSelected(Passenger passenger)
    {
        
    }

    public override bool CanBoard(Passenger passenger, Bus bus)
    {
        ReservedBusTrait reservedBusTrait = bus.GetComponent<ReservedBusTrait>();

        if (reservedBusTrait == null)
            return false;

        if (passenger.Color != bus.Color)
            return false;

        return true;
    }
    public override bool CanMove(Passenger passenger, Bus bus)
    {
        return true;
    }

    public bool IsReserved => isReserved;
}