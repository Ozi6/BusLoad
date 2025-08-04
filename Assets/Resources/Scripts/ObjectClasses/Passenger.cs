using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public PassengerColor Color { get; set; }
    public Vector2Int GridPosition { get; set; }
    public List<PassengerTrait> traits = new List<PassengerTrait>();
    public Renderer bodyRenderer;

    private void OnMouseDown()
    {
        Debug.Log("passenger clicked");
        foreach (PassengerTrait trait in traits)
            trait.OnSelected(this);

        PassengerController.Instance.SelectPassenger(this);
    }

    public bool CanBoardBus(Bus bus)
    {
        if (Color != bus.Color)
            return false;

        foreach (PassengerTrait trait in traits)
            if (!trait.CanBoard(this, bus))
                return false;

        foreach (BusTrait trait in bus.traits)
            if (!trait.CanAcceptPassenger(bus, this))
                return false;

        return true;
    }

    public void SetColor(PassengerColor color)
    {
        Color = color;
        if (bodyRenderer != null)
        {
            Color renderColor = color switch
            {
                PassengerColor.Red => UnityEngine.Color.red,
                PassengerColor.Blue => UnityEngine.Color.blue,
                PassengerColor.Green => UnityEngine.Color.green,
                PassengerColor.Yellow => UnityEngine.Color.yellow,
                PassengerColor.Purple => new Color(0.5f, 0f, 1f),
                _ => UnityEngine.Color.white
            };
            bodyRenderer.material.color = renderColor;
        }
    }
}