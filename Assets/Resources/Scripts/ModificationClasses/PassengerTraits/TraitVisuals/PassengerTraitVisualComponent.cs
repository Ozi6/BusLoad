using UnityEngine;

public abstract class TraitVisualComponent : MonoBehaviour
{
    public abstract void UpdateVisual(PassengerTrait trait);
    public abstract void ResetVisual();
}