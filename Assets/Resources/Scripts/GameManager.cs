using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelData levelData;
    public GameObject passengerPrefab;
    public Transform gridParent;
    public Dictionary<Vector2Int, Passenger> gridPassengers = new Dictionary<Vector2Int, Passenger>();

    private void Awake() => Instance = this;

    private void Start()
    {
        Invoke(nameof(SpawnPassengers), 0.1f);
    }

    public bool HasPassengerAt(Vector2Int position) => gridPassengers.ContainsKey(position);

    public void RemovePassengerFromGrid(Vector2Int position)
    {
        gridPassengers.Remove(position);
        PassengerController.Instance.FloodFillInteractable(position);
    }

    private void SpawnPassengers()
    {
        if (levelData == null)
            return;

        foreach (PassengerData data in levelData.passengers)
        {
            GameObject passengerObj = Instantiate(passengerPrefab, gridParent);
            Passenger passenger = passengerObj.GetComponent<Passenger>();
            passenger.SetColor(data.color);
            passenger.GridPosition = data.gridPosition;
            passenger.SetInteractable(false);
            gridPassengers[data.gridPosition] = passenger;

            foreach (string traitType in data.traitTypes)
            {
                System.Type type = System.Type.GetType(traitType);
                if (type != null)
                {
                    PassengerTrait trait = (PassengerTrait)passengerObj.AddComponent(type);
                    passenger.traits.Add(trait);
                    if (trait is RopedTrait roped)
                        roped.Initialize(passenger);
                }
            }

            Vector3 worldPos = new Vector3(data.gridPosition.x, 0.5f, data.gridPosition.y);
            passengerObj.transform.position = worldPos;
        }

        UpdateInteractablePassengers();
    }

    private void UpdateInteractablePassengers()
    {
        int maxY = int.MinValue;
        foreach (Vector2Int pos in gridPassengers.Keys)
            if (pos.y > maxY)
                maxY = pos.y;

        foreach (var pair in gridPassengers)
        {
            bool isInteractable = pair.Key.y == maxY;
            pair.Value.SetInteractable(isInteractable);
        }
    }

    public void RespawnPassengers()
    {
        foreach (var passenger in gridPassengers.Values)
            if (passenger != null)
                DestroyImmediate(passenger.gameObject);
        gridPassengers.Clear();
        SpawnPassengers();
    }

    public Vector2Int GetGridBounds()
    {
        int maxX = 0, maxY = 0;
        foreach (Vector2Int pos in gridPassengers.Keys)
        {
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }
        return new Vector2Int(maxX + 1, maxY + 1);
    }
}