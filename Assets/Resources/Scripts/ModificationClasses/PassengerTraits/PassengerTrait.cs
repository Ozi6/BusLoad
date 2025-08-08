using UnityEngine;

public abstract class PassengerTrait : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        PassengerEvents.OnPassengerSelected += OnNearbyPassengerSelected;
    }

    protected virtual void OnDisable()
    {
        PassengerEvents.OnPassengerSelected -= OnNearbyPassengerSelected;
    }

    public abstract void OnSelected(Passenger passenger);
    public abstract bool CanBoard(Passenger passenger, Bus bus);

    protected virtual void OnNearbyPassengerSelected(Vector2Int position) { }
}