using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using UnityEngine;

public class NetworkEntityManager : MonoBehaviour
{
    public NetworkManager networkManager;

    public List<NetworkEntity> entities;

    public List<NetworkEntity> movableEntities;

    public List<NetworkEntity> spawnableEntities;
    private bool _isQuitting;
    private int _uniqueID = 10000;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        entities = new List<NetworkEntity>();
        StartCoroutine(SendContinuousPosition(2));
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private IEnumerator SendContinuousPosition(int delay)
    {
        yield return new WaitForSeconds(delay);

        if (networkManager.networkMap.gameplayState.Equals(1) && movableEntities.Count > 0)
            foreach (var e in movableEntities)
            {
                var position = e.transform.position;
                var euler = e.transform.eulerAngles;

                SendMessageToClient($"EntityPosition@{e.name}@{e.entityId}" +
                                    $"@{position.x}" +
                                    $"@{position.y}" +
                                    $"@{position.z}" +
                                    $"@{euler.x}" +
                                    $"@{euler.y}" +
                                    $"@{euler.z}");
            }

        if (_isQuitting) yield break;
        StartCoroutine(SendContinuousPosition(2));
    }

    public void SendClientEntitiesStatus(NetPeer peer)
    {
        foreach (var e in entities.Where(e => e))
            SendMessageToClient($"EntitySetActive@{e.name}@{e.entityId}@{e.gameObject.activeSelf}", peer);
    }

    public NetworkEntity SpawnEntity(Vector3 position, Quaternion rotation, string entityName)
    {
        var entity = spawnableEntities.Find(g => g.name == entityName);
        if (!entity) return null;

        var networkEntity = Instantiate(entity, position, rotation).GetComponent<NetworkEntity>();

        networkEntity.entityId = _uniqueID.ToString();
        _uniqueID++;

        position = networkEntity.position;

        networkEntity.name = entity.name;

        SendMessageToClient($"EntitySpawn@{networkEntity.name}@{networkEntity.entityId}" +
                            $"@{position.x}@{position.y}@{position.z}");

        return networkEntity;
    }

    public void SpawnRagdoll(Player deadPlayer)
    {
        var entity = spawnableEntities.Find(g => g.name == "Net Ragdoll");

        var networkEntity = Instantiate(entity, deadPlayer.Body.transform.position, deadPlayer.Body.transform.rotation).GetComponent<NetworkEntity>();

        networkEntity.entityId = _uniqueID.ToString();
        _uniqueID++;

        var position = networkEntity.position;

        networkEntity.name = entity.name;

        SendMessageToClient($"RagdollEntitySpawn@{networkEntity.name}@{networkEntity.entityId}" +
                            $"@{position.x}@{position.y}@{position.z}" +
                            $"@{deadPlayer.GetPeerId()}");
    }

    public void RemoveEntity(NetworkEntity entity)
    {
        entities.RemoveAll(x => x.entityId == entity.entityId);
        movableEntities.RemoveAll(x => x.entityId == entity.entityId);
        SendMessageToClient($"EntityDespawn@{entity.entityId}");
        Destroy(entity.gameObject);
    }

    public void SendMessageToClient(string message, NetPeer peer)
    {
        networkManager.SendMessageToClient(message, peer);
    }

    public void SendMessageToClient(string message)
    {
        networkManager.SendMessageToClient(message);
    }

    public void Clear()
    {
        entities.Clear();
        movableEntities.Clear();
    }
}