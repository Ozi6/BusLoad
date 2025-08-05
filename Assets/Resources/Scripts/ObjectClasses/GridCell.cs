using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int gridPosition;

    private void OnMouseEnter()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.HighlightCell(gridPosition);
    }

    private void OnMouseExit()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.ClearHighlight();
    }
}
