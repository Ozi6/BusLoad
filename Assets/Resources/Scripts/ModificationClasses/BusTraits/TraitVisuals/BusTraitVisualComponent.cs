using UnityEngine;

public abstract class BusTraitVisualComponent : MonoBehaviour
{
    public abstract void UpdateVisual(BusTrait trait);
    public abstract void ResetVisual();
}