using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteNetLib;
using UnityEngine;
using UnityEngine.Networking.Types;

public class NetworkEntityManager : MonoBehaviour
{
    public NetworkManager _networkManager;

    public List<NetworkEntity> entities;

    public List<NetworkEntity> movableEntities;

    public List<NetworkEntity> spawnableEntities;
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
                SendMessageToClient($"EntityPosition@{e.name}@{e.entityId}" +
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

    public void SpawnEntity(string[] data)
    {
        var entity = spawnableEntities.Find(g => g.name == data[1]);
        if (!entity) return;
        
        var position = new Vector3(float.Parse(data[2]), float.Parse(data[3]), float.Parse(data[4]));
        var direction = new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7]));

        var networkEntity = Instantiate(entity, position, Quaternion.identity).GetComponent<NetworkEntity>();
        networkEntity.entityId = entities.Count.ToString();

        position = networkEntity.position;
        var eulerAngles = networkEntity.euler;

        networkEntity.name = entity.name;
        
        SendMessageToClient($"EntitySpawn@{networkEntity.name}@{networkEntity.entityId}" +
                            $"@{position.x}@{position.y}@{position.z}" +
                            $"@{eulerAngles.x}@{eulerAngles.y}@{eulerAngles.z}");
    }

    public NetworkEntity SpawnEntity(Vector3 position, string entityName)
    {
        var entity = spawnableEntities.Find(g => g.name == entityName);
        if (!entity) return null;
        
        var networkEntity = Instantiate(entity, position, Quaternion.identity).GetComponent<NetworkEntity>();
        networkEntity.entityId = entities.Count.ToString();

        position = networkEntity.position;
        var eulerAngles = networkEntity.euler;

        networkEntity.name = entity.name;
        
        SendMessageToClient($"EntitySpawn@{networkEntity.name}@{networkEntity.entityId}" +
                            $"@{position.x}@{position.y}@{position.z}" +
                            $"@{eulerAngles.x}@{eulerAngles.y}@{eulerAngles.z}");

        return networkEntity;
    }

    public void AddEntity(NetworkEntity entity)
    {
        entities.Add(entity);
        Debug.Log($"Added Entity with ID {entity.entityId} and Type {entity.entityType}");
    }

    public void RemoveEntity(NetworkEntity entity)
    {
        entities.RemoveAll(x => x.entityId == entity.entityId);
        movableEntities.RemoveAll(x => x.entityId == entity.entityId);
        Destroy(entity.gameObject);
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