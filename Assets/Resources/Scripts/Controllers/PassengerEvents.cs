using System;
using UnityEngine;

public static class PassengerEvents
{
    public static event Action<Vector2Int> OnPassengerSelected;
    public static event Action<Passenger> OnPassengerTraitsChanged;

    public static void TriggerPassengerSelected(Vector2Int position) => OnPassengerSelected?.Invoke(position);
    public static void TriggerPassengerTraitsChanged(Passenger passenger) => OnPassengerTraitsChanged?.Invoke(passenger);
}