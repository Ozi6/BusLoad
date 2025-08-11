using UnityEngine;

public class FrozenTrait : PassengerTrait
{
    [SerializeField] private int frozenTurns = 4;
    private bool tooFrozen = true;
    private Passenger owner;

    private void Awake()
    {
        owner = GetComponent<Passenger>();
    }

    public void Configure(TraitConfiguration config)
    {
        if (config.traitType == "FrozenTrait")
            frozenTurns = config.intValue;
    }

    public override void OnSelected(Passenger passenger)
    {
        if (frozenTurns > 0)
            return;
    }

    public override bool CanBoard(Passenger passenger, Bus bus)
    {
        return frozenTurns <= 0;
    }
    public override bool CanMove(Passenger passenger, Bus bus)
    {
        return frozenTurns <= 0;
    }

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null || tooFrozen)
            return;
        frozenTurns--;
        UpdateVisualIndicator();
        if (frozenTurns <= 0)
            owner.RemoveTrait(this);
    }

    protected override void OnPassengerReachedByFlood(Passenger passenger)
    {
        if (passenger == owner)
            tooFrozen = false;
    }
}