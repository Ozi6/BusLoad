using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

struct MovementRequest
{
    public MovementType type;
    public Vector3 targetPosition;
    public Transform targetTransform;
    public List<Vector2Int> path;
    public float speed;
    public System.Action onComplete;

    public static MovementRequest CreatePosition(Vector3 target, float speed, System.Action onComplete)
    {
        return new MovementRequest
        {
            type = MovementType.Position,
            targetPosition = target,
            speed = speed,
            onComplete = onComplete
        };
    }

    public static MovementRequest CreateTransform(Transform target, float speed, System.Action onComplete)
    {
        return new MovementRequest
        {
            type = MovementType.Transform,
            targetTransform = target,
            speed = speed,
            onComplete = onComplete
        };
    }

    public static MovementRequest CreatePath(List<Vector2Int> path, float speed, System.Action onComplete)
    {
        return new MovementRequest
        {
            type = MovementType.Path,
            path = new List<Vector2Int>(path),
            speed = speed,
            onComplete = onComplete
        };
    }
}