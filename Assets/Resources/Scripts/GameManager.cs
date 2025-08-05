using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelData levelData;
    public GameObject passengerPrefab;
    public GameObject gridSquarePrefab;
    public Transform gridParent;
    public Dictionary<Vector2Int, Passenger> gridPassengers = new Dictionary<Vector2Int, Passenger>();
    private const int GRID_SIZE = 12;
    private const float GRID_SPACING = 2f;

    private void Awake() => Instance = this;

    private void Start()
    {
        InitializeGrid();
        Invoke(nameof(SpawnPassengers), 0.1f);
    }

    private void InitializeGrid()
    {
        if (gridSquarePrefab == null)
            return;

        if (gridParent == null)
            gridParent = new GameObject("GridParent").transform;

        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                Vector3 worldPos = new Vector3(gridParent.transform.position.x + x * GRID_SPACING, 0.1f, gridParent.transform.position.z + y * GRID_SPACING);
                GameObject gridSquare = Instantiate(gridSquarePrefab, worldPos, Quaternion.identity, gridParent);
                gridSquare.name = $"GridSquare_{x}_{y}";

                gridSquare.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            }
        }
    }

    public bool HasPassengerAt(Vector2Int position) => gridPassengers.ContainsKey(position);

    public void RemovePassengerFromGrid(Vector2Int position)
    {
        gridPassengers.Remove(position);
        FloodFillManager.Instance.FloodFillInteractable(position);
    }

    private void SpawnPassengers()
    {
        if (levelData == null)
            return;

        foreach (PassengerData data in levelData.passengers)
        {
            if (data.gridPosition.x < 0 || data.gridPosition.x >= GRID_SIZE ||
                data.gridPosition.y < 0 || data.gridPosition.y >= GRID_SIZE)
                continue;

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

            Vector3 worldPos = new Vector3(
                gridParent.transform.position.x + data.gridPosition.x * GRID_SPACING,
                0.5f,
                gridParent.transform.position.z + data.gridPosition.y * GRID_SPACING
            );
            passengerObj.transform.position = worldPos;
        }

        FloodFillManager.Instance.InitializeInteractablePassengers();
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
        return new Vector2Int(GRID_SIZE, GRID_SIZE);
    }
}