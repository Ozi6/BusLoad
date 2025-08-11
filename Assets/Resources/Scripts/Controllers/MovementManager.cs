using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance;
    public float defaultMoveSpeed = 15f;
    private Dictionary<GameObject, Coroutine> activeMovements = new Dictionary<GameObject, Coroutine>();
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

        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }

        Coroutine newMovement = StartCoroutine(MoveToPosition(obj, targetPosition, speed <= 0 ? defaultMoveSpeed : speed, onComplete));
        activeMovements[obj] = newMovement;
    }

    public void MoveGradual(GameObject obj, Transform target, float speed = 0f, System.Action onComplete = null)
    {
        if (obj == null || target == null)
            return;

        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }

        Coroutine newMovement = StartCoroutine(MoveToPosition(obj, target.position, speed <= 0 ? defaultMoveSpeed : speed, onComplete));
        activeMovements[obj] = newMovement;
    }

    public void MoveAlongPath(GameObject obj, List<Vector2Int> path, float speed = 0f, System.Action onComplete = null)
    {
        if (obj == null || path == null || path.Count <= 1)
        {
            onComplete?.Invoke();
            return;
        }

        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }

        Coroutine newMovement = StartCoroutine(MoveAlongPathCoroutine(obj, path, speed <= 0 ? defaultMoveSpeed : speed, onComplete));
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

        obj.transform.position = targetPosition;
    }

    public void MoveInstant(GameObject obj, Transform target)
    {
        if (obj == null || target == null)
            return;

        if (activeMovements.ContainsKey(obj) && activeMovements[obj] != null)
        {
            StopCoroutine(activeMovements[obj]);
            activeMovements.Remove(obj);
        }

        obj.transform.position = target.position;
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
    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPosition, float speed, System.Action onComplete)
    {
        while (Vector3.Distance(obj.transform.position, targetPosition) > 0.1f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        obj.transform.position = targetPosition;
        activeMovements.Remove(obj);
        onComplete?.Invoke();
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

        activeMovements.Remove(obj);
        onComplete?.Invoke();
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