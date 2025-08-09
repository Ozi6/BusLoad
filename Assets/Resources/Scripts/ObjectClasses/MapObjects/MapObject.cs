using UnityEngine;

public abstract class MapObject : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public virtual bool BlocksPath => true;
    public virtual bool BlocksFlood => true;
    public virtual void OnReachedByFlood() { }
}