using UnityEngine;

public class CloakedTrait : PassengerTrait
{
    private Passenger owner;
    [SerializeField] private bool isCloaked = true;

    private void Start()
    {
        owner = GetComponent<Passenger>();
    }

    public override void OnSelected(Passenger passenger)
    {
        if (isCloaked)
            return;
    }

    public override bool CanBoard(Passenger passenger, Bus bus)
    {
        return !isCloaked;
    }

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null)
            return;
        if(clickedPosition != owner.Position)
            ToggleCloak();
    }

    private void ToggleCloak()
    {
        isCloaked = !isCloaked;
    }

    public bool IsCloaked => isCloaked;
}