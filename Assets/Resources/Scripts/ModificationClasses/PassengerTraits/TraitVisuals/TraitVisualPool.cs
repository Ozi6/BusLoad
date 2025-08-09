using System.Collections.Generic;
using UnityEngine;

public class TraitVisualPool : MonoBehaviour
{
    [System.Serializable]
    public class TraitVisualData
    {
        public string traitTypeName;
        public GameObject prefab;
        public int poolSize = 10;
    }

    public static TraitVisualPool Instance { get; private set; }

    [SerializeField] private TraitVisualData[] traitVisuals;
    private Dictionary<System.Type, Queue<GameObject>> pools = new Dictionary<System.Type, Queue<GameObject>>();
    private Dictionary<System.Type, GameObject> prefabMap = new Dictionary<System.Type, GameObject>();
    private Dictionary<GameObject, System.Type> activeVisuals = new Dictionary<GameObject, System.Type>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
            Destroy(gameObject);
    }

    private void InitializePools()
    {
        foreach (var data in traitVisuals)
        {
            System.Type traitType = System.Type.GetType(data.traitTypeName);
            if (traitType == null)
                continue;

            pools[traitType] = new Queue<GameObject>();
            prefabMap[traitType] = data.prefab;

            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject obj = Instantiate(data.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                obj.GetComponent<TraitVisualComponent>().ResetVisual();
                pools[traitType].Enqueue(obj);
            }
        }
    }

    public GameObject GetVisual(System.Type traitType, PassengerTrait trait)
    {
        if (!pools.ContainsKey(traitType))
            return null;

        GameObject visual;
        if (pools[traitType].Count > 0)
            visual = pools[traitType].Dequeue();
        else
            visual = Instantiate(prefabMap[traitType]);

        visual.SetActive(true);
        activeVisuals[visual] = traitType;
        visual.GetComponent<TraitVisualComponent>().UpdateVisual(trait);
        return visual;
    }

    public void UpdateVisual(GameObject visual, PassengerTrait trait)
    {
        if (visual != null)
            visual.GetComponent<TraitVisualComponent>().UpdateVisual(trait);
    }

    public void ReturnVisual(GameObject visual)
    {
        if (visual == null)
            return;

        System.Type traitType = activeVisuals[visual];
        activeVisuals.Remove(visual);

        visual.SetActive(false);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.GetComponent<TraitVisualComponent>().ResetVisual();

        if (pools.ContainsKey(traitType))
            pools[traitType].Enqueue(visual);
        else
            Destroy(visual);
    }
}