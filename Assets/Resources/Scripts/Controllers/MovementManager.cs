using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance;
    public float defaultMoveSpeed = 15f;
    private Dictionary<GameObject, Coroutine> activeMovements = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Queue<MovementRequest>> queuedMovements = new Dictionary<GameObject, Queue<MovementRequest>>();
    private const float GRID_SPACING = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void MoveGradual(GameObject obj, Vector3 targetPosition, float speed = 0f, System.Action onComplete = null)
    {
        if (obj == null)
            return;
        var request = MovementRequest.CreatePosition(targetPosition, speed <= 0 ? defaultMoveSpeed : speed, onComplete);
        QueueOrStartMovement(obj, request);
    }

    public void MoveGradual(GameObject obj, Transform target, float speed = 0f, System.Action onComplete = null)
    {
        if (obj == null || target == null)
            return;
        var request = MovementRequest.CreateTransform(target, speed <= 0 ? defaultMoveSpeed : speed, onComplete);
        QueueOrStartMovement(obj, request);
    }

    public void MoveAlongPath(GameObject obj, List<Vector2Int> path, float speed = 0f, System.Action onComplete = null)
    {
        if (obj == null || path == null || path.Count <= 1)
        {
            onComplete?.Invoke();
            return;
        }
        var request = MovementRequest.CreatePath(path, speed <= 0 ? defaultMoveSpeed : speed, onComplete);
        QueueOrStartMovement(obj, request);
    }

    private void QueueOrStartMovement(GameObject obj, MovementRequest request)
    {
        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            if (!queuedMovements.ContainsKey(obj))
                queuedMovements[obj] = new Queue<MovementRequest>();
            queuedMovements[obj].Enqueue(request);
            return;
        }
        StartMovementRequest(obj, request);
    }

    private void StartMovementRequest(GameObject obj, MovementRequest request)
    {
        Coroutine newMovement = null;
        switch (request.type)
        {
            case MovementType.Position:
                newMovement = StartCoroutine(MoveToPosition(obj, request.targetPosition, request.speed, request.onComplete));
                break;
            case MovementType.Transform:
                newMovement = StartCoroutine(MoveToPosition(obj, request.targetTransform.position, request.speed, request.onComplete));
                break;
            case MovementType.Path:
                newMovement = StartCoroutine(MoveAlongPathCoroutine(obj, request.path, request.speed, request.onComplete));
                break;
        }
        if (newMovement != null)
            activeMovements[obj] = newMovement;
    }

    public void MoveInstant(GameObject obj, Vector3 targetPosition)
    {
        if (obj == null)
            return;
        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }
        if (queuedMovements.ContainsKey(obj))
            queuedMovements[obj].Clear();
        obj.transform.position = targetPosition;
    }

    public void MoveInstant(GameObject obj, Transform target)
    {
        if (obj == null || target == null)
            return;
        MoveInstant(obj, target.position);
    }

    public bool HasActiveMovement(GameObject obj)
    {
        return activeMovements.ContainsKey(obj) && activeMovements[obj] != null;
    }

    public void CancelMovement(GameObject obj)
    {
        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }
        ProcessNextQueuedMovement(obj);
    }

    private void OnMovementComplete(GameObject obj, System.Action originalCallback)
    {
        activeMovements.Remove(obj);
        originalCallback?.Invoke();
        ProcessNextQueuedMovement(obj);
    }

    private void ProcessNextQueuedMovement(GameObject obj)
    {
        if (queuedMovements.ContainsKey(obj) && queuedMovements[obj].Count > 0)
        {
            var nextRequest = queuedMovements[obj].Dequeue();
            StartMovementRequest(obj, nextRequest);
        }
    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPosition, float speed, System.Action onComplete)
    {
        while (Vector3.Distance(obj.transform.position, targetPosition) > 0.1f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        obj.transform.position = targetPosition;
        OnMovementComplete(obj, onComplete);
    }

    private IEnumerator MoveAlongPathCoroutine(GameObject obj, List<Vector2Int> path, float speed, System.Action onComplete)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 targetWorldPos = GridToWorldPosition(path[i]);
            while (Vector3.Distance(obj.transform.position, targetWorldPos) > 0.1f)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetWorldPos, speed * Time.deltaTime);
                yield return null;
            }
            obj.transform.position = targetWorldPos;
        }
        OnMovementComplete(obj, onComplete);
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            GameManager.Instance.gridParent.transform.position.x + gridPos.x * GRID_SPACING,
            0.5f,
            GameManager.Instance.gridParent.transform.position.z + gridPos.y * GRID_SPACING
        );
    }

    public List<GameObject> GetAllMovingObjects()
    {
        return new List<GameObject>(activeMovements.Keys);
    }
}