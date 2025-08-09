using System.Collections.Generic;
using UnityEngine;

public class Passenger : MapObject
{
    public PassengerColor Color { get; set; }
    public List<PassengerTrait> traits = new List<PassengerTrait>();
    public List<Renderer> bodyRenderers = new List<Renderer>();
    private Collider _collider;
    private bool _isInteractable = false;
    public bool IsInteractable => _isInteractable;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (bodyRenderers.Count == 0)
            bodyRenderers.AddRange(GetComponentsInChildren<Renderer>());
    }

    void Update()
    {
        if (!_isInteractable)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Passenger passenger = hit.collider.GetComponentInParent<Passenger>();
                if (passenger == this)
                {
                    for (int i = passenger.traits.Count - 1; i >= 0; i--)
                        passenger.traits[i].OnSelected(passenger);
                    PassengerController.Instance.SelectPassenger(passenger);
                }
            }
        }
    }

    public void RemoveTrait(PassengerTrait trait)
    {
        if (traits.Remove(trait))
        {
            Destroy(trait);
            PassengerEvents.TriggerPassengerTraitsChanged(this);
        }
    }

    public bool CanBoardBus(Bus bus)
    {
        if (Color != bus.Color)
            return false;
        if (!CanTraitMove(bus))
            return false;
        foreach (BusTrait trait in bus.traits)
            if (!trait.CanAcceptPassenger(bus, this))
                return false;
        return true;
    }

    public bool CanTraitMove(Bus bus)
    {
        foreach (PassengerTrait trait in traits)
            if (!trait.CanBoard(this, bus))
                return false;
        return true;
    }

    public void SetColor(PassengerColor color)
    {
        Color = color;
        Color renderColor = color switch
        {
            PassengerColor.Red => UnityEngine.Color.red,
            PassengerColor.Blue => UnityEngine.Color.blue,
            PassengerColor.Green => UnityEngine.Color.green,
            PassengerColor.Yellow => UnityEngine.Color.yellow,
            PassengerColor.Purple => new Color(0.5f, 0f, 1f),
            _ => UnityEngine.Color.white
        };
        foreach (var renderer in bodyRenderers)
            if (renderer != null)
                renderer.material.color = renderColor;
    }

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
        if (_collider != null)
            _collider.enabled = isInteractable;
    }

    public override void OnReachedByFlood()
    {
        SetInteractable(true);
    }
}