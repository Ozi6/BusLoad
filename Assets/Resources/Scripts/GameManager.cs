using System.Collections.Generic;
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

    public GridManager gridManager;

    private int GridWidth => levelData?.gridWidth ?? 12;
    private int GridHeight => levelData?.gridHeight ?? 12;
    private float GridSpacing => levelData?.gridSpacing ?? 2f;

    private void Awake() => Instance = this;

    private void Start()
    {
        levelData = LevelManager.GetSelectedLevel();
        gridManager = new GridManager(gridParent, gridSquarePrefab, gridObjects, GridWidth, GridHeight, GridSpacing);
        gridManager.InitializeGrid();
        Invoke(nameof(SpawnGridObjects), 0.1f);
    }

    private void SpawnGridObjects()
    {
        if (levelData == null) return;
        foreach (var p in levelData.passengers) SpawnPassenger(p);
        foreach (var w in levelData.walls) SpawnWall(w);
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

                var config = data.traitConfigs.Find(c => c.traitType == traitType);
                if (config != null)
                    trait.GetType().GetMethod("Configure")?.Invoke(trait, new object[] { config });
            }
        }

        passengerObj.transform.position = new Vector3(
            gridParent.position.x + data.gridPosition.x * GridSpacing,
            0.5f,
            gridParent.position.z + data.gridPosition.y * GridSpacing
        );
        gridObjects[data.gridPosition] = passenger;
    }

    private void SpawnWall(WallData data)
    {
        GameObject wallObj = Instantiate(wallPrefab, gridParent);
        Wall wall = wallObj.GetComponent<Wall>();
        wall.Position = data.gridPosition;

        wallObj.transform.position = new Vector3(
            gridParent.position.x + data.gridPosition.x * GridSpacing,
            0.5f,
            gridParent.position.z + data.gridPosition.y * GridSpacing
        );
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

    public Vector2Int GetGridBounds() => new Vector2Int(GridWidth, GridHeight);

    public List<Vector2Int> FindPathToHighestEmpty(Vector2Int startPos)
    {
        AStarPathfinder pathfinder = new AStarPathfinder(GridWidth, GridHeight, gridManager);
        return pathfinder.FindPathToHighestEmptySquare(startPos);
    }
}
