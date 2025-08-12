using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    public static PassengerController Instance;
    public Queue<Passenger> waitingQueue = new Queue<Passenger>();
    public int maxQueueSize = 5;

    private bool isProcessingQueue = false;
    private HashSet<Passenger> passengersBeingProcessed = new HashSet<Passenger>();

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
        if (passengersBeingProcessed.Contains(passenger))
            return;

        Bus currentBus = BusController.Instance.CurrentBus;

        if (BusController.Instance.IsBusAtBoardingPoint && currentBus != null && passenger.CanBoardBus(currentBus))
        {
            if (currentBus.TryReserveSpot(passenger))
            {
                passenger.SetInteractable(false);
                passengersBeingProcessed.Add(passenger);
                StartCoroutine(MovePassengerToBoarding(passenger, currentBus));
                PassengerEvents.TriggerPassengerSelected(passenger.Position);
            }
        }
        else if (!QueueManager.Instance.IsQueueFull() && passenger.CanTraitMove(currentBus))
        {
            if (!QueueManager.Instance.IsPassengerInQueue(passenger))
            {
                if (QueueManager.Instance.AddToQueue(passenger))
                {
                    passengersBeingProcessed.Add(passenger);
                    StartCoroutine(MovePassengerToQueue(passenger));
                    PassengerEvents.TriggerPassengerSelected(passenger.Position);
                }
            }
        }
    }

    private IEnumerator MovePassengerToBoarding(Passenger passenger, Bus bus)
    {
        Vector2Int startPos = passenger.Position;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);
        GameManager.Instance.gridManager.RemoveOccupantFromGrid(passenger.Position);

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

        passengersBeingProcessed.Remove(passenger);
    }

    private IEnumerator MovePassengerToQueue(Passenger passenger)
    {
        Vector2Int startPos = passenger.Position;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);
        GameManager.Instance.gridManager.RemoveOccupantFromGrid(passenger.Position);

        bool movementComplete = false;
        bool movementCanceled = false;

        if (pathToHighest.Count > 1)
        {
            MovementManager.Instance.MoveAlongPath(passenger.gameObject, pathToHighest, 0f, () => {
                movementComplete = true;
            });

            yield return new WaitUntil(() => movementComplete || !MovementManager.Instance.HasActiveMovement(passenger.gameObject));

            if (!movementComplete)
                movementCanceled = true;
        }

        if (!movementCanceled)
            QueueManager.Instance.MovePassengerToQueuePosition(passenger);
        else
            QueueManager.Instance.MovePassengerToQueuePosition(passenger);

        passengersBeingProcessed.Remove(passenger);
    }

    public void ProcessQueue()
    {
        if (isProcessingQueue || !BusController.Instance.IsBusAtBoardingPoint || BusController.Instance.CurrentBus == null)
            return;

        Bus currentBus = BusController.Instance.CurrentBus;

        if (!currentBus.CanAcceptMorePassengers())
            return;

        isProcessingQueue = true;
        StartCoroutine(ProcessQueueCoroutine(currentBus));
    }

    private IEnumerator ProcessQueueCoroutine(Bus bus)
    {
        QueueManager.Instance.ProcessQueueForBus(bus);
        yield return new WaitForEndOfFrame();
        isProcessingQueue = false;
    }

    public void ResetProcessingState()
    {
        passengersBeingProcessed.Clear();
        isProcessingQueue = false;
    }
}