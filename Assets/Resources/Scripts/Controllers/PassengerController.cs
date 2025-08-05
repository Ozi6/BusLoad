using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerController : MonoBehaviour
{
    public static PassengerController Instance;
    public Queue<Passenger> waitingQueue = new Queue<Passenger>();
    public int maxQueueSize = 5;
    private List<RopedTrait> ropedTraits = new List<RopedTrait>();
    private const float GRID_SPACING = 2f;

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
            StartCoroutine(MovePassengerToBoarding(passenger, currentBus));
        else
            StartCoroutine(MovePassengerToQueue(passenger));
    }

    private IEnumerator MovePassengerToBoarding(Passenger passenger, Bus bus)
    {
        Vector2Int startPos = passenger.GridPosition;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);

        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);

        if (pathToHighest.Count > 1)
            yield return StartCoroutine(MoveAlongPath(passenger.gameObject, pathToHighest));

        Vector3 boardingPosition = BusController.Instance.busBoardingPoint.position + new Vector3(0, 0, -2f);
        yield return StartCoroutine(MoveToWorldPosition(passenger.gameObject, boardingPosition));

        bus.AddPassenger(passenger);
    }

    private IEnumerator MovePassengerToQueue(Passenger passenger)
    {
        Vector2Int startPos = passenger.GridPosition;
        List<Vector2Int> pathToHighest = GameManager.Instance.FindPathToHighestEmpty(startPos);

        GameManager.Instance.RemovePassengerFromGrid(passenger.GridPosition);

        if (pathToHighest.Count > 1)
            yield return StartCoroutine(MoveAlongPath(passenger.gameObject, pathToHighest));

        if (QueueManager.Instance.IsQueueFull())
            QueueManager.Instance.RemoveOldestIfFull();
        QueueManager.Instance.AddToQueue(passenger);
    }

    private IEnumerator MoveAlongPath(GameObject passengerObj, List<Vector2Int> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetWorldPos = GridToWorldPosition(path[i]);
            yield return StartCoroutine(MoveToWorldPosition(passengerObj, targetWorldPos));
        }
    }

    private IEnumerator MoveToWorldPosition(GameObject obj, Vector3 targetPosition)
    {
        float moveSpeed = 5f;

        while (Vector3.Distance(obj.transform.position, targetPosition) > 0.1f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        obj.transform.position = targetPosition;
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            GameManager.Instance.gridParent.transform.position.x + gridPos.x * GRID_SPACING,
            0.5f,
            GameManager.Instance.gridParent.transform.position.z + gridPos.y * GRID_SPACING
        );
    }

    public void FloodFillInteractable(Vector2Int startPos)
    {
        Vector2Int gridSize = GameManager.Instance.GetGridBounds();
        Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startPos);
        visited[startPos] = true;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (GameManager.Instance.HasPassengerAt(current))
            {
                Passenger passenger = GameManager.Instance.gridPassengers[current];
                passenger.SetInteractable(true);
                continue;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < gridSize.x && next.y >= 0 && next.y < gridSize.y && !visited.ContainsKey(next))
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }
    }

    public void ProcessQueue()
    {
        if (!BusController.Instance.IsBusAtBoardingPoint || BusController.Instance.CurrentBus == null)
            return;
        Bus currentBus = BusController.Instance.CurrentBus;
        QueueManager.Instance.ProcessQueueForBus(currentBus);
    }
}