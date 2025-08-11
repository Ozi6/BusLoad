public class ReservedBusTrait : BusTrait
{
    private int reservedPassengerCapacity = 1;
    private void Awake()
    {
        owner = GetComponent<Bus>();
    }

    private void Start()
    {
        UpdateVisualIndicator();
    }

    public void Configure(TraitConfiguration config)
    {
        if (config.traitType == "ReservedBusTrait")
            reservedPassengerCapacity = config.intValue;
    }

    public override bool CanAcceptPassenger(Bus bus, Passenger passenger)
    {
        ReservedTrait passengerReservedTrait = passenger.GetComponent<ReservedTrait>();

        int currentReservedCount = GetReservedPassengerCount(bus);
        int busCapacity = bus.passengerPositions.Length;
        int currentPassengerCount = bus.Passengers.Count;

        if (passengerReservedTrait != null && passengerReservedTrait.IsReserved)
        {
            if (currentReservedCount < reservedPassengerCapacity)
                return passenger.Color == bus.Color;
            else
                return false;
        }

        int reservedLeft = reservedPassengerCapacity - currentReservedCount;
        int freeSpots = busCapacity - currentPassengerCount;

        if (freeSpots > reservedLeft)
            return passenger.Color == bus.Color;

        return false;
    }

    private int GetReservedPassengerCount(Bus bus)
    {
        int count = 0;
        foreach (Passenger passenger in bus.Passengers)
        {
            ReservedTrait reservedTrait = passenger.GetComponent<ReservedTrait>();
            if (reservedTrait != null && reservedTrait.IsReserved)
                count++;
        }
        return count;
    }

    public int ReservedPassengerCapacity => reservedPassengerCapacity;
    public int RemainingReservedSeats => ReservedPassengerCapacity - GetReservedPassengerCount(owner);
}