using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelData levelData;
    public GameObject passengerPrefab;
    public GameObject wallPrefab;
    public GameObject tunnelPrefab;
    public GameObject gridSquarePrefab;
    public Transform gridParent;
    public Dictionary<Vector2Int, MapObject> gridObjects = new Dictionary<Vector2Int, MapObject>();

    private int GridWidth => levelData?.gridWidth ?? 12;
    private int GridHeight => levelData?.gridHeight ?? 12;
    private float GridSpacing => levelData?.gridSpacing ?? 2f;

    private void Awake() => Instance = this;

    private void Start()
    {
        levelData = LevelManager.GetSelectedLevel();

        InitializeGrid();
        Invoke(nameof(SpawnGridObjects), 0.1f);
    }

    private void InitializeGrid()
    {
        if (gridSquarePrefab == null)
            return;
        if (gridParent == null)
            gridParent = new GameObject("GridParent").transform;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    gridParent.transform.position.x + x * GridSpacing,
                    0.1f,
                    gridParent.transform.position.z + y * GridSpacing
                );
                GameObject gridSquare = Instantiate(gridSquarePrefab, worldPos, Quaternion.identity, gridParent);
                gridSquare.name = $"GridSquare{x}_{y}";
                gridSquare.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            }
        }
    }

    public bool HasOccupantAt(Vector2Int position) => gridObjects.ContainsKey(position);

    public bool HasPassengerAt(Vector2Int position) => gridObjects.TryGetValue(position, out var obj) && obj is Passenger;

    public void RemoveOccupantFromGrid(Vector2Int position)
    {
        if (gridObjects.TryGetValue(position, out var obj))
        {
            gridObjects.Remove(position);
            TunnelManager.Instance.CheckForTunnelSpawns(position);
            FloodFillManager.Instance.FloodFillInteractable(position);
        }
    }

    private void SpawnGridObjects()
    {
        if (levelData == null)
            return;

        foreach (PassengerData data in levelData.passengers)
        {
            if (data.gridPosition.x < 0 || data.gridPosition.x >= GridWidth ||
                data.gridPosition.y < 0 || data.gridPosition.y >= GridHeight)
                continue;

            SpawnPassenger(data);
        }
        foreach (WallData data in levelData.walls)
        {
            if (data.gridPosition.x < 0 || data.gridPosition.x >= GridWidth ||
                data.gridPosition.y < 0 || data.gridPosition.y >= GridHeight)
                continue;

            SpawnWall(data);
        }
        TunnelManager.Instance.SpawnTunnelsFromLevelData(levelData.tunnels);

        FloodFillManager.Instance.InitializeInteractablePassengers();
    }

    private void SpawnPassenger(PassengerData data)
    {
        GameObject passengerObj = Instantiate(passengerPrefab, gridParent);
        Passenger passenger = passengerObj.GetComponent<Passenger>();
        passenger.SetColor(data.color);
        passenger.Position = data.gridPosition;
        passenger.SetInteractable(false);

        foreach (string traitType in data.traitTypes)
        {
            System.Type type = System.Type.GetType(traitType);
            if (type != null && type.IsSubclassOf(typeof(PassengerTrait)))
            {
                PassengerTrait trait = (PassengerTrait)passengerObj.AddComponent(type);
                passenger.traits.Add(trait);

                var config = data.traitConfigs.FirstOrDefault(c => c.traitType == traitType);
                if (config != null)
                {
                    var configMethod = trait.GetType().GetMethod("Configure");
                    if (configMethod != null)
                        configMethod.Invoke(trait, new object[] { config });
                }
            }
        }

        Vector3 worldPos = new Vector3(
            gridParent.transform.position.x + data.gridPosition.x * GridSpacing,
            0.5f,
            gridParent.transform.position.z + data.gridPosition.y * GridSpacing
        );
        passengerObj.transform.position = worldPos;
        gridObjects[data.gridPosition] = passenger;
    }

    private void SpawnWall(WallData data)
    {
        GameObject wallObj = Instantiate(wallPrefab, gridParent);
        Wall wall = wallObj.GetComponent<Wall>();
        wall.Position = data.gridPosition;

        Vector3 worldPos = new Vector3(
            gridParent.transform.position.x + data.gridPosition.x * GridSpacing,
            0.5f,
            gridParent.transform.position.z + data.gridPosition.y * GridSpacing
        );
        wallObj.transform.position = worldPos;
        gridObjects[data.gridPosition] = wall;
    }

    public void RespawnPassengers()
    {
        foreach (var kv in new Dictionary<Vector2Int, MapObject>(gridObjects))
        {
            if (kv.Value is Passenger passenger)
            {
                DestroyImmediate(passenger.gameObject);
                gridObjects.Remove(kv.Key);
            }
        }
        TunnelManager.Instance.ResetAllTunnels();
        SpawnGridObjects();
    }

    public Vector2Int GetGridBounds()
    {
        return new Vector2Int(GridWidth, GridHeight);
    }

    public List<Vector2Int> FindPathToHighestEmpty(Vector2Int startPos)
    {
        AStarPathfinder pathfinder = new AStarPathfinder(GridWidth, GridHeight, this);
        return pathfinder.FindPathToHighestEmptySquare(startPos);
    }
}