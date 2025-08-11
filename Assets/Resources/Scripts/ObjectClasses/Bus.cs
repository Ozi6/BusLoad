using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public PassengerColor Color { get; set; }
    public List<Passenger> Passengers { get; set; } = new List<Passenger>();
    public List<BusTrait> traits = new List<BusTrait>();
    public Renderer bodyRenderer;
    public float moveSpeed = 15f;
    public Transform[] passengerPositions = new Transform[3];
    private int currentPassengerIndex = 0;
    private HashSet<Passenger> passengersBoarding = new HashSet<Passenger>();
    private bool isBoardingLocked = false;

    public bool CanAcceptMorePassengers()
    {
        return !isBoardingLocked && (Passengers.Count + passengersBoarding.Count) < 3;
    }

    public bool TryReserveSpot(Passenger passenger)
    {
        if (!CanAcceptMorePassengers() || passengersBoarding.Contains(passenger))
            return false;
        passengersBoarding.Add(passenger);
        return true;
    }

    public void ReleaseReservation(Passenger passenger)
    {
        passengersBoarding.Remove(passenger);
    }

    public void AddPassenger(Passenger passenger)
    {
        if (!passenger.CanBoardBus(this) || Passengers.Count >= 3)
        {
            passengersBoarding.Remove(passenger);
            return;
        }

        passengersBoarding.Remove(passenger);
        Passengers.Add(passenger);
        passenger.transform.SetParent(transform);

        NotifyTraitsOnBusBoarded(passenger);

        Vector3 boardingPosition = transform.position + new Vector3(0, 0, 2f);
        MovementManager.Instance.MoveGradual(passenger.gameObject, boardingPosition, 5f, () =>
        {
            if (currentPassengerIndex < passengerPositions.Length)
            {
                MovementManager.Instance.MoveInstant(passenger.gameObject, passengerPositions[currentPassengerIndex]);
                currentPassengerIndex++;
                if (Passengers.Count >= 3 && currentPassengerIndex >= 3)
                {
                    isBoardingLocked = true;
                    BusController.Instance.DepartBus();
                }
            }
        });
    }

    private void NotifyTraitsOnBusBoarded(Passenger passenger)
    {
        foreach (BusTrait trait in traits)
            if (trait != null && trait.enabled)
                trait.OnBusBoarded(this, passenger);
    }

    public void SetColor(PassengerColor color)
    {
        Color = color;
        if (bodyRenderer != null)
        {
            Color renderColor = color switch
            {
                PassengerColor.Red => UnityEngine.Color.red,
                PassengerColor.Blue => UnityEngine.Color.blue,
                PassengerColor.Green => UnityEngine.Color.green,
                PassengerColor.Yellow => UnityEngine.Color.yellow,
                PassengerColor.Purple => new Color(0.5f, 0f, 1f),
                _ => UnityEngine.Color.white
            };
            bodyRenderer.material.color = renderColor;
        }
    }

    public bool IsBusFull()
    {
        return (Passengers.Count + passengersBoarding.Count) > passengerPositions.Length;
    }

    public void ResetBoardingState()
    {
        passengersBoarding.Clear();
        isBoardingLocked = false;
        currentPassengerIndex = 0;
        Passengers.Clear();
    }
}