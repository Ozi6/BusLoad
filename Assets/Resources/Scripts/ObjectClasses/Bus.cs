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

    public void AddPassenger(Passenger passenger)
    {
        if (!passenger.CanBoardBus(this) || Passengers.Count >= 3)
            return;

        Passengers.Add(passenger);
        passenger.transform.SetParent(transform);
        GameManager.Instance.RemoveOccupantFromGrid(passenger.Position);

        Vector3 boardingPosition = transform.position + new Vector3(0, 0, 2f);
        MovementManager.Instance.MoveGradual(passenger.gameObject, boardingPosition, 5f, () =>
        {
            if (currentPassengerIndex < passengerPositions.Length)
            {
                MovementManager.Instance.MoveInstant(passenger.gameObject, passengerPositions[currentPassengerIndex]);
                currentPassengerIndex++;
                if (Passengers.Count >= 3 && currentPassengerIndex >= 3)
                    BusController.Instance.DepartBus();
            }
        });
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
}