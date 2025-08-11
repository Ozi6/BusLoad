using UnityEngine;
using System.Collections.Generic;

public class BusTraitVisualPool : MonoBehaviour
{
    [System.Serializable]
    public class TraitVisualData
    {
        public string traitTypeName;
        public GameObject prefab;
        public int poolSize = 5;
    }

    public static BusTraitVisualPool Instance { get; private set; }

    [SerializeField] private TraitVisualData[] traitVisuals;
    private Dictionary<System.Type, Queue<GameObject>> pools = new();
    private Dictionary<System.Type, GameObject> prefabMap = new();
    private Dictionary<GameObject, System.Type> activeVisuals = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else Destroy(gameObject);
    }

    private void InitializePools()
    {
        foreach (var data in traitVisuals)
        {
            System.Type traitType = System.Type.GetType(data.traitTypeName);
            if (traitType == null) continue;

            pools[traitType] = new Queue<GameObject>();
            prefabMap[traitType] = data.prefab;

            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject obj = Instantiate(data.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                obj.GetComponent<BusTraitVisualComponent>().ResetVisual();
                pools[traitType].Enqueue(obj);
            }
        }
    }

    public GameObject GetVisual(System.Type traitType, BusTrait trait)
    {
        if (!pools.ContainsKey(traitType)) return null;

        GameObject visual = pools[traitType].Count > 0
            ? pools[traitType].Dequeue()
            : Instantiate(prefabMap[traitType]);

        visual.transform.SetParent(trait.GetComponent<Bus>().transform, false);
        visual.SetActive(true);
        activeVisuals[visual] = traitType;
        visual.GetComponent<BusTraitVisualComponent>().UpdateVisual(trait);
        return visual;
    }


    public void UpdateVisual(GameObject visual, BusTrait trait)
    {
        if (visual != null)
            visual.GetComponent<BusTraitVisualComponent>().UpdateVisual(trait);
    }

    public void ReturnVisual(GameObject visual)
    {
        if (visual == null) return;

        System.Type traitType = activeVisuals[visual];
        activeVisuals.Remove(visual);

        visual.SetActive(false);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.GetComponent<BusTraitVisualComponent>().ResetVisual();

        if (pools.ContainsKey(traitType))
            pools[traitType].Enqueue(visual);
        else
            Destroy(visual);
    }
}
