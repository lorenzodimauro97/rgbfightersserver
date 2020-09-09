using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using UnityEngine;

public class NetworkEntityManager : MonoBehaviour
{
    public NetworkManager networkManager;

    public Dictionary<string, NetworkEntity> entities, movableEntities;

    public List<NetworkEntity> spawnableEntities;
    
    private bool _isQuitting;
    private int _uniqueID = 10000;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        entities = new Dictionary<string, NetworkEntity>();
        movableEntities = new Dictionary<string, NetworkEntity>();
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
                var position = e.Value.transform.position;
                var euler = e.Value.transform.eulerAngles;

                SendMessageToClient($"EntityPosition@{e.Value.name}@{e.Value.entityId}" +
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
        foreach (var e in entities)
            SendMessageToClient($"EntitySetActive@{e.Value.name}@{e.Value.entityId}@{e.Value.gameObject.activeSelf}", peer);
    }

    public NetworkEntity SpawnEntity(Vector3 position, Quaternion rotation, string entityName, Player ownerPlayer)
    {
        var entity = spawnableEntities.Find(g => g.name == entityName);
        if (!entity) return null;

        var networkEntity = Instantiate(entity, position, rotation).GetComponent<NetworkEntity>();

        networkEntity.entityId = _uniqueID.ToString();
        _uniqueID++;

        position = networkEntity.position;

        networkEntity.name = entity.name;

        networkEntity.ownerPlayer = ownerPlayer;

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
        entities.Remove(entity.entityId);
        movableEntities.Remove(entity.entityId);
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