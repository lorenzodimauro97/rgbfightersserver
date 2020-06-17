using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class NetworkEntityManager : MonoBehaviour
{
    private NetworkManager _networkManager;

    public List<NetworkEntity> entities;
    void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
        entities = new List<NetworkEntity>();
    }

    public void AddEntity(NetworkEntity entity)
    {
        entities.Add(entity);
        Debug.Log($"Added Entity with ID {entity.entityId} and Type {entity.entityType}");
    }

    public void SendMessageToClient(string message, NetPeer peer) => _networkManager.SendMessageToClient(message, peer);
    public void SendMessageToClient(string message) => _networkManager.SendMessageToClient(message);
}
