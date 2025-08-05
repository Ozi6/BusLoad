using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelData levelData;
    public GameObject passengerPrefab;
    public Transform gridParent;
    private Dictionary<Vector2Int, Passenger> gridPassengers = new Dictionary<Vector2Int, Passenger>();

    private void Awake() => Instance = this;

    private void Start()
    {
        Invoke(nameof(SpawnPassengers), 0.1f);
    }

    public bool HasPassengerAt(Vector2Int position) => gridPassengers.ContainsKey(position);

    public void RemovePassengerFromGrid(Vector2Int position) => gridPassengers.Remove(position);

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

            Vector3 worldPos = GridManager.Instance != null ?
                GridManager.Instance.GridToWorldPosition(data.gridPosition) + Vector3.up * 1f :
                new Vector3(data.gridPosition.x, 0.5f, data.gridPosition.y);

            passengerObj.transform.position = worldPos;
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
}