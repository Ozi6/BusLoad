using UnityEngine;

public class BombedTrait : PassengerTrait
{
    [SerializeField] private int initialCountdown = 5;
    private Passenger owner;
    private int currentCountdown;
    private bool isActivated = false;
    private bool isExploded = false;

    private void Awake()
    {
        owner = GetComponent<Passenger>();
        currentCountdown = initialCountdown;
    }

    public void Configure(TraitConfiguration config)
    {
        if (config.traitType == "BombedTrait")
        {
            initialCountdown = config.intValue;
            currentCountdown = initialCountdown;
        }
    }

    public override void OnSelected(Passenger passenger)
    {
        if (isExploded)
            return;
        passenger.RemoveTrait(this);
    }

    public override bool CanBoard(Passenger passenger, Bus bus)
    {
        return !isExploded;
    }

    protected override void OnNearbyPassengerSelected(Vector2Int clickedPosition)
    {
        if (owner == null || isExploded) return;
        if (isActivated) DecrementCountdown();
    }

    protected override void OnPassengerReachedByFlood(Passenger passenger)
    {
        if (passenger == owner && !isExploded && !isActivated)
            ActivateBomb();
    }

    private void ActivateBomb()
    {
        isActivated = true;
        UpdateVisualIndicator();
    }

    private void DecrementCountdown()
    {
        currentCountdown--;
        UpdateVisualIndicator();
        if (currentCountdown <= 0)
            ExplodeBomb();
    }

    private void ExplodeBomb()
    {
        isExploded = true;
        isActivated = false;
        UpdateVisualIndicator();
        OnBombExploded();
    }

    protected virtual void OnBombExploded() { }

    public int GetCurrentCountdown() => currentCountdown;
    public bool IsActivated() => isActivated;
    public bool IsExploded() => isExploded;

    public void SetInitialCountdown(int newCountdown)
    {
        initialCountdown = newCountdown;
        if (!isActivated)
            currentCountdown = initialCountdown;
    }
}