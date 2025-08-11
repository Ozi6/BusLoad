using UnityEngine;

public class CloakedTrait : PassengerTrait
{
    private Passenger owner;
    [SerializeField] private bool isCloaked = true;

    private void Start()
    {
        owner = GetComponent<Passenger>();
    }

    public void Configure(TraitConfiguration config)
    {
        if (config.traitType.Equals("CloakedTrait"))
            isCloaked = config.boolValue;
    }

    public override void OnSelected(Passenger passenger)
    {
        if (isCloaked)
            return;
        passenger.RemoveTrait(this);
    }

    public override bool CanBoard(Passenger passenger, Bus bus)
    {
        return !isCloaked;
    }
    public override bool CanMove(Passenger passenger, Bus bus)
    {
        return !isCloaked;
    }

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null)
            return;
        if (clickedPosition != owner.Position) ToggleCloak();
    }

    private void ToggleCloak()
    {
        isCloaked = !isCloaked;
        UpdateVisualIndicator();
    }

    public bool IsCloaked => isCloaked;
}