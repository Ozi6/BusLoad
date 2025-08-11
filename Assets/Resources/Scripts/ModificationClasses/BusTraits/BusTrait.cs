using UnityEngine;

public abstract class BusTrait : MonoBehaviour
{
    protected Bus owner;

    public abstract bool CanAcceptPassenger(Bus bus, Passenger passenger);

    public virtual void OnBusBoarded(Bus bus, Passenger passenger)
    {

    }

    protected GameObject visualIndicator;

    protected virtual void OnEnable()
    {
        CreateVisualIndicator();
    }

    protected virtual void OnDisable()
    {
        ReturnVisualIndicator();
    }

    protected virtual void CreateVisualIndicator()
    {
        visualIndicator = BusTraitVisualPool.Instance.GetVisual(GetType(), this);
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
            BusTraitVisualPool.Instance.ReturnVisual(visualIndicator);
            visualIndicator = null;
        }
    }

    protected void UpdateVisualIndicator()
    {
        if (visualIndicator != null)
            BusTraitVisualPool.Instance.UpdateVisual(visualIndicator, this);
    }

    protected virtual Vector3 GetVisualOffset() => Vector3.zero;
}