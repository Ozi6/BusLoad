using UnityEngine;

public class BombedTrait : PassengerTrait
{
    [SerializeField] private int initialCountdown = 5;
    private Passenger owner;
    private int currentCountdown;
    private bool isActivated = false;
    private bool isExploded = false;

    private void Start()
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
        else if (IsNeighboring(owner.Position, clickedPosition)) ActivateBomb();
    }

    private bool IsNeighboring(Vector2Int pos1, Vector2Int pos2)
    {
        int deltaX = Mathf.Abs(pos1.x - pos2.x);
        int deltaY = Mathf.Abs(pos1.y - pos2.y);
        return (deltaX <= 1 && deltaY <= 1) && !(deltaX == 0 && deltaY == 0);
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