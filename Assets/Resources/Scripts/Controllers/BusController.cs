using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    public static BusController Instance;
    public Bus CurrentBus { get; private set; }
    public LevelData levelData;
    public Transform busSpawnPoint;
    public Transform busBoardingPoint;
    public Transform busDeparturePoint;
    public GameObject busPrefab;
    private Queue<BusData> busQueue = new Queue<BusData>();
    public bool IsBusAtBoardingPoint { get; private set; }

    private void Awake()
    {
        Instance = this;
        levelData = LevelManager.GetSelectedLevel();
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
        {
            IsBusAtBoardingPoint = false;
            MovementManager.Instance.MoveGradual(CurrentBus.gameObject, busDeparturePoint, CurrentBus.moveSpeed, () =>
            {
                Destroy(CurrentBus.gameObject);
                CurrentBus = null;
                SpawnNextBus();
            });
        }
    }

    private void SpawnNextBus()
    {
        if (busQueue.Count == 0)
            return;

        BusData data = busQueue.Dequeue();
        GameObject busObj = Instantiate(busPrefab, busSpawnPoint.position, busSpawnPoint.rotation);
        CurrentBus = busObj.GetComponent<Bus>();
        CurrentBus.SetColor(data.color);

        foreach (string traitType in data.traitTypes)
        {
            System.Type type = System.Type.GetType(traitType);
            if (type != null && type.IsSubclassOf(typeof(BusTrait)))
            {
                BusTrait trait = (BusTrait)busObj.AddComponent(type);
                CurrentBus.traits.Add(trait);

                var config = data.traitConfigs.Find(c => c.traitType == traitType);
                if (config != null)
                    trait.GetType().GetMethod("Configure")?.Invoke(trait, new object[] { config });

                BusTraitVisualComponent visual = BusTraitVisualPool.Instance
                    .GetVisual(type, trait)
                    .GetComponent<BusTraitVisualComponent>();

                visual.transform.SetParent(busObj.transform, false);
                visual.UpdateVisual(trait);
            }
        }

        MovementManager.Instance.MoveGradual(busObj, busBoardingPoint, CurrentBus.moveSpeed, () =>
        {
            IsBusAtBoardingPoint = true;
            PassengerController.Instance.ProcessQueue();
        });
    }
}