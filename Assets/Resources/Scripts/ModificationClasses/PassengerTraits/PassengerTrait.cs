using UnityEngine;

public abstract class PassengerTrait : MonoBehaviour
{
    public abstract void OnSelected(Passenger passenger);
    public abstract bool CanBoard(Passenger passenger, Bus bus);
}