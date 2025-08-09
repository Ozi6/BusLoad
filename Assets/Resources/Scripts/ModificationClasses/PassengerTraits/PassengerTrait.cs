using UnityEngine;

public abstract class PassengerTrait : MonoBehaviour
{
    protected GameObject visualIndicator;

    protected virtual void OnEnable()
    {
        PassengerEvents.OnPassengerSelected += OnNearbyPassengerSelected;
        CreateVisualIndicator();
    }

    protected virtual void OnDisable()
    {
        PassengerEvents.OnPassengerSelected -= OnNearbyPassengerSelected;
        ReturnVisualIndicator();
    }

    protected virtual void CreateVisualIndicator()
    {
        visualIndicator = TraitVisualPool.Instance.GetVisual(GetType(), this);
        if (visualIndicator != null)
        {
            visualIndicator.transform.SetParent(transform);
            visualIndicator.transform.localPosition = GetVisualOffset();
        }
    }

    protected virtual void ReturnVisualIndicator()
    {
        if (visualIndicator != null)
        {
            TraitVisualPool.Instance.ReturnVisual(visualIndicator);
            visualIndicator = null;
        }
    }

    protected void UpdateVisualIndicator()
    {
        if (visualIndicator != null)
            TraitVisualPool.Instance.UpdateVisual(visualIndicator, this);
    }

    protected virtual Vector3 GetVisualOffset() => Vector3.zero;

    public abstract void OnSelected(Passenger passenger);
    public abstract bool CanBoard(Passenger passenger, Bus bus);
    protected virtual void OnNearbyPassengerSelected(Vector2Int position) { }
}