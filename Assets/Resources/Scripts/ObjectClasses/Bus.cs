using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    public PassengerColor Color { get; set; }
    public List<Passenger> Passengers { get; set; } = new List<Passenger>();
    public List<BusTrait> traits = new List<BusTrait>();
    public Renderer bodyRenderer;
    public float moveSpeed = 5f;

    public void AddPassenger(Passenger passenger)
    {
        if (!passenger.CanBoardBus(this) || Passengers.Count >= 3)
            return;

        Passengers.Add(passenger);
        passenger.transform.SetParent(transform);
        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);

        if (Passengers.Count >= 3)
            BusController.Instance.DepartBus();
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

    public void DepartToDestination(Transform destination)
    {
        StartCoroutine(MoveToDestination(destination));
    }

    private System.Collections.IEnumerator MoveToDestination(Transform destination)
    {
        while (Vector3.Distance(transform.position, destination.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}