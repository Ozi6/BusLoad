using System.Collections.Generic;
using UnityEngine;

public class TunnelManager : MonoBehaviour
{
    public static TunnelManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void CheckForTunnelSpawns(Vector2Int emptyPos)
    {
        foreach (var dir in DirectionVectors.CardinalDirections)
        {
            Vector2Int tunnelPos = emptyPos - dir;
            if (GameManager.Instance.gridObjects.TryGetValue(tunnelPos, out var obj) && obj is Tunnel tunnel && tunnel.SpawnDirection == dir)
            {
                if (!tunnel.HasPassengersLeft)
                    continue;
                PassengerData template = tunnel.PeekNextPassenger();
                if (template == null)
                    continue;
                tunnel.ConsumeNextPassenger();
                SpawnPassengerFromTunnel(template, emptyPos);
                break;
            }
        }
    }

    private void SpawnPassengerFromTunnel(PassengerData template, Vector2Int position)
    {
        GameObject passengerObj = Instantiate(GameManager.Instance.passengerPrefab, GameManager.Instance.gridParent);
        Passenger passenger = passengerObj.GetComponent<Passenger>();
        passenger.SetColor(template.color);
        passenger.Position = position;
        passenger.SetInteractable(false);

        foreach (string traitType in template.traitTypes)
        {
            System.Type type = System.Type.GetType(traitType);
            if (type != null && type.IsSubclassOf(typeof(PassengerTrait)))
            {
                PassengerTrait trait = (PassengerTrait)passengerObj.AddComponent(type);
                passenger.traits.Add(trait);
            }
        }

        Vector3 worldPos = new Vector3(
            GameManager.Instance.gridParent.transform.position.x + position.x * 2f,
            0.5f,
            GameManager.Instance.gridParent.transform.position.z + position.y * 2f
        );
        passengerObj.transform.position = worldPos;
        GameManager.Instance.gridObjects[position] = passenger;
    }

    public void SpawnTunnelsFromLevelData(List<TunnelData> tunnelData)
    {
        foreach (TunnelData data in tunnelData)
        {
            if (data.gridPosition.x < 0 || data.gridPosition.x >= 12 || // GRID_SIZE
                data.gridPosition.y < 0 || data.gridPosition.y >= 12)
                continue;

            GameObject tunnelObj = Instantiate(GameManager.Instance.tunnelPrefab, GameManager.Instance.gridParent);
            Tunnel tunnel = tunnelObj.GetComponent<Tunnel>();
            tunnel.Position = data.gridPosition;
            tunnel.SpawnDirection = data.direction;

            tunnel.PassengerQueue = new List<PassengerData>();
            foreach (var passengerData in data.passengerQueue)
            {
                tunnel.PassengerQueue.Add(new PassengerData
                {
                    color = passengerData.color,
                    traitTypes = new List<string>(passengerData.traitTypes)
                });
            }

            Vector3 worldPos = new Vector3(
                GameManager.Instance.gridParent.transform.position.x + data.gridPosition.x * 2f,
                0.5f,
                GameManager.Instance.gridParent.transform.position.z + data.gridPosition.y * 2f
            );
            tunnelObj.transform.position = worldPos;
            GameManager.Instance.gridObjects[data.gridPosition] = tunnel;
        }
    }

    public void ResetAllTunnels()
    {
        foreach (var kv in GameManager.Instance.gridObjects)
            if (kv.Value is Tunnel tunnel)
                tunnel.ResetSpawnIndex();
    }
}