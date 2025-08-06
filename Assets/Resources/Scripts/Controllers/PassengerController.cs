using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    public static PassengerController Instance;
    public Queue<Passenger> waitingQueue = new Queue<Passenger>();
    public int maxQueueSize = 5;
    private List<RopedTrait> ropedTraits = new List<RopedTrait>();

    private void Awake() => Instance = this;

    public void RegisterRoped(RopedTrait roped) => ropedTraits.Add(roped);
    public void UnregisterRoped(RopedTrait roped) => ropedTraits.Remove(roped);

    public void CheckRopedNearby(Vector2Int position)
    {
        foreach (RopedTrait roped in ropedTraits)
            roped.CheckUntie(position);
    }

    public void SelectPassenger(Passenger passenger)
    {
        CheckRopedNearby(passenger.GridPosition);
        Bus currentBus = BusController.Instance.CurrentBus;

        if (BusController.Instance.IsBusAtBoardingPoint && currentBus != null && passenger.CanBoardBus(currentBus))
        {
            passenger.SetInteractable(false);
            StartCoroutine(MovePassengerToBoarding(passenger, currentBus));
        }
        else if (!QueueManager.Instance.IsQueueFull())
        {
            if (!QueueManager.Instance.IsPassengerInQueue(passenger))
                QueueManager.Instance.AddToQueue(passenger);
            StartCoroutine(MovePassengerToQueue(passenger));
        }
    }

    private IEnumerator MovePassengerToBoarding(Passenger passenger, Bus bus)
    {
        Vector2Int startPos = passenger.GridPosition;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);

        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);

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
        Vector2Int startPos = passenger.GridPosition;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);

        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);
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