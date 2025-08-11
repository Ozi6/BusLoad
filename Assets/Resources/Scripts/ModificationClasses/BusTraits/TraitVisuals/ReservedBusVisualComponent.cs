using TMPro;
using UnityEngine;

public class ReservedBusVisualComponent : BusTraitVisualComponent
{
    [SerializeField] private TextMeshPro text;

    public override void UpdateVisual(BusTrait trait)
    {
        if (trait is ReservedBusTrait reservedTrait)
            text.text = $"R{reservedTrait.RemainingReservedSeats}";
    }

    public override void ResetVisual()
    {
        text.text = "";
    }
}
