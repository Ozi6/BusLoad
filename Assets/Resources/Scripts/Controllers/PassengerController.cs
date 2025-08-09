using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    public static PassengerController Instance;
    public Queue<Passenger> waitingQueue = new Queue<Passenger>();
    public int maxQueueSize = 5;
    private void Awake() => Instance = this;
    private void OnEnable()
    {
        PassengerEvents.OnPassengerSelected += HandlePassengerSelected;
    }
    private void OnDisable()
    {
        PassengerEvents.OnPassengerSelected -= HandlePassengerSelected;
    }
    private void HandlePassengerSelected(Vector2Int position)
    {
    }
    public void SelectPassenger(Passenger passenger)
    {
        Bus currentBus = BusController.Instance.CurrentBus;
        if (BusController.Instance.IsBusAtBoardingPoint && currentBus != null && passenger.CanBoardBus(currentBus))
        {
            passenger.SetInteractable(false);
            StartCoroutine(MovePassengerToBoarding(passenger, currentBus));
            PassengerEvents.TriggerPassengerSelected(passenger.Position);
        }
        else if (!QueueManager.Instance.IsQueueFull() && passenger.CanTraitMove(currentBus))
        {
            if (!QueueManager.Instance.IsPassengerInQueue(passenger))
                QueueManager.Instance.AddToQueue(passenger);
            StartCoroutine(MovePassengerToQueue(passenger));
            PassengerEvents.TriggerPassengerSelected(passenger.Position);
        }
    }
    private IEnumerator MovePassengerToBoarding(Passenger passenger, Bus bus)
    {
        Vector2Int startPos = passenger.Position;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);
        GameManager.Instance.RemoveOccupantFromGrid(passenger.Position);
        bool pathMovementComplete = false;
        bool boardingMovementComplete = false;
        if (pathToHighest.Count > 1)
        {
            MovementManager.Instance.MoveAlongPath(passenger.gameObject, pathToHighest, 0f, () => { pathMovementComplete = true; });
            yield return new WaitUntil(() => pathMovementComplete);
        }
        else
            pathMovementComplete = true;
        Vector3 boardingPosition = BusController.Instance.busBoardingPoint.position + new Vector3(0, 0, -2f);
        MovementManager.Instance.MoveGradual(passenger.gameObject, boardingPosition, 0f, () => { boardingMovementComplete = true; });
        yield return new WaitUntil(() => boardingMovementComplete);
        bus.AddPassenger(passenger);
    }
    private IEnumerator MovePassengerToQueue(Passenger passenger)
    {
        Vector2Int startPos = passenger.Position;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);
        GameManager.Instance.RemoveOccupantFromGrid(passenger.Position);
        bool movementComplete = false;
        if (pathToHighest.Count > 1)
        {
            MovementManager.Instance.MoveAlongPath(passenger.gameObject, pathToHighest, 0f, () => {
                movementComplete = true;
            });
            yield return new WaitUntil(() => movementComplete);
        }
        QueueManager.Instance.MovePassengerToQueuePosition(passenger);
    }
    public void ProcessQueue()
    {
        if (!BusController.Instance.IsBusAtBoardingPoint || BusController.Instance.CurrentBus == null)
            return;
        Bus currentBus = BusController.Instance.CurrentBus;
        QueueManager.Instance.ProcessQueueForBus(currentBus);
    }
}