using UnityEngine;

public class PassengerHover : MonoBehaviour
{
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] outlineMaterials;
    private bool isHovered = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        outlineMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            outlineMaterials[i] = CreateOutlineMaterial(originalMaterials[i]);
        }
    }

    private Material CreateOutlineMaterial(Material original)
    {
        Material outline = new Material(original);
        outline.color = Color.white;
        outline.SetFloat("_Metallic", 0f);
        outline.SetFloat("_Smoothness", 1f);
        outline.EnableKeyword("_EMISSION");
        outline.SetColor("_EmissionColor", Color.white * 0.5f);
        return outline;
    }

    void OnMouseEnter()
    {
        SetHovered(true);
    }

    void OnMouseExit()
    {
        SetHovered(false);
    }

    void SetHovered(bool hovered)
    {
        if (isHovered == hovered)
            return;
        isHovered = hovered;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = hovered ? outlineMaterials[i] : originalMaterials[i];
        }
    }
}