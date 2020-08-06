using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;

public class NetworkEntityManager : MonoBehaviour
{
    private bool _isQuitting;
    public NetworkManager _networkManager;

    public List<NetworkEntity> entities;

    public List<NetworkEntity> movableEntities;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
        entities = new List<NetworkEntity>();
        new Task(() => SendContinuousPosition(3000), TaskCreationOptions.LongRunning).Start();
    }

    private async void SendContinuousPosition(int delay)
    {
        while (!_isQuitting)
        {
            var tdelay = Task.Delay(delay);

            foreach (var message in movableEntities.Select(e => $"EntityPosition@{e.entityId}" +
                                                                $"@{e.position.x}" +
                                                                $"@{e.position.y}" +
                                                                $"@{e.position.z}" +
                                                                $"@{e.euler.x}" +
                                                                $"@{e.euler.y}" +
                                                                $"@{e.euler.z}"))
                SendMessageToClient(message);
            await tdelay;
        }
    }

    public void SendClientEntitiesStatus(NetPeer peer)
    {
        foreach (var message in entities.Select(e => $"EntitySetActive@{e.entityId}@{e.gameObject.activeSelf}"))
            SendMessageToClient(message, peer);
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

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    public void Clear()
    {
        entities.Clear();
        movableEntities.Clear();
    }
}