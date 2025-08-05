using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementManager : MonoBehaviour
{
    public static MovementManager Instance;
    public float defaultMoveSpeed = 5f;
    private Dictionary<GameObject, Coroutine> activeMovements = new Dictionary<GameObject, Coroutine>();

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
}