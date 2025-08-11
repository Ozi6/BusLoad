using UnityEngine;

public class FrozenVisualComponent : TraitVisualComponent
{
    [SerializeField] private GameObject iceBlock;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = iceBlock.transform.localScale;
    }

    public override void UpdateVisual(PassengerTrait trait)
    {
        Vector3 scale = iceBlock.transform.localScale;
        scale.y /= 2f;
        iceBlock.transform.localScale = scale;
    }

    public override void ResetVisual()
    {
        iceBlock.transform.localScale = originalScale;
    }
}
