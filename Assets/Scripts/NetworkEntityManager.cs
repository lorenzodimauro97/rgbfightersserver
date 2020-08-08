using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;

public class NetworkEntityManager : MonoBehaviour
{
    public NetworkManager _networkManager;

    public List<NetworkEntity> entities;

    public List<NetworkEntity> movableEntities;
    private bool _isQuitting;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
        entities = new List<NetworkEntity>();
        new Task(() => SendContinuousPosition(3000), TaskCreationOptions.LongRunning).Start();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private async void SendContinuousPosition(int delay)
    {
        while (!_isQuitting)
        {
            var tdelay = Task.Delay(delay);

            foreach (var e in movableEntities)
            {
                SendMessageToClient($"EntityPosition@{e.entityId}" +
                                    $"@{e.transform.position.x}" +
                                    $"@{transform.position.y}" +
                                    $"@{e.transform.position.z}" +
                                    $"@{e.transform.eulerAngles.x}" +
                                    $"@{e.transform.eulerAngles.y}" +
                                    $"@{e.transform.eulerAngles.z}");
            }
            await tdelay;
        }
    }

    public void SendClientEntitiesStatus(NetPeer peer)
    {
        foreach (var e in movableEntities)
        {
            if(!e) continue;
            SendMessageToClient($"EntitySetActive@{e.entityId}@{e.gameObject.activeSelf}", peer);
        }
    }

    public void AddEntity(NetworkEntity entity)
    {
        entities.Add(entity);
        Debug.Log($"Added Entity with ID {entity.entityId} and Type {entity.entityType}");
    }

    public void SendMessageToClient(string message, NetPeer peer)
    {
        _networkManager.SendMessageToClient(message, peer);
    }

    public void SendMessageToClient(string message)
    {
        _networkManager.SendMessageToClient(message);
    }

    public void Clear()
    {
        entities.Clear();
        movableEntities.Clear();
    }
}