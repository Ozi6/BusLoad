using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public static BusController Instance;
    public Bus CurrentBus { get; private set; }
    public LevelData levelData;
    public Transform busSpawnPoint;
    public Transform busDeparturePoint;
    public GameObject busPrefab;
    private Queue<BusData> busQueue = new Queue<BusData>();

    private void Awake()
    {
        Instance = this;
        InitializeBuses();
        SpawnNextBus();
    }

    private void InitializeBuses()
    {
        foreach (BusData busData in levelData.buses)
            busQueue.Enqueue(busData);
    }

    public void DepartBus()
    {
        if (CurrentBus != null)
            CurrentBus.DepartToDestination(busDeparturePoint);
        SpawnNextBus();
        PassengerController.Instance.ProcessQueue();
    }

    private void SpawnNextBus()
    {
        if (busQueue.Count == 0) return;

        BusData data = busQueue.Dequeue();
        GameObject busObj = Instantiate(busPrefab, busSpawnPoint.position, busSpawnPoint.rotation);
        CurrentBus = busObj.GetComponent<Bus>();
        CurrentBus.SetColor(data.color);

        foreach (string traitType in data.traitTypes)
        {
            System.Type type = System.Type.GetType(traitType);
            if (type != null)
            {
                BusTrait trait = (BusTrait)busObj.AddComponent(type);
                CurrentBus.traits.Add(trait);
            }
        }
    }
}