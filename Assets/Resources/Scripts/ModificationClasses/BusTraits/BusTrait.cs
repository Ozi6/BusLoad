using UnityEngine;

public abstract class BusTrait : MonoBehaviour
{
    public abstract bool CanAcceptPassenger(Bus bus, Passenger passenger);
}