using UnityEngine;

public class CloakVisualComponent : TraitVisualComponent
{
    [SerializeField] private GameObject cloakObject;
    [SerializeField] private Material visibleMaterial;
    [SerializeField] private Material invisibleMaterial;
    private Renderer cloakRenderer;

    private void Awake()
    {
        cloakRenderer = cloakObject.GetComponent<Renderer>();
    }

    public override void UpdateVisual(PassengerTrait trait)
    {
        CloakedTrait cloakTrait = (CloakedTrait)trait;
        bool isCloaked = cloakTrait.IsCloaked;

        cloakObject.SetActive(true);
        cloakRenderer.material = isCloaked ? invisibleMaterial : visibleMaterial;
    }

    public override void ResetVisual()
    {
        cloakObject.SetActive(false);
    }
}