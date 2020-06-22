using System;
using LiteNetLib;
using UnityEngine;

[Serializable]
public class Player : MonoBehaviour
{
    public float Health { get; set; }
    public NetPeer Peer { get; private set; }
    public GameObject Body { get; set; }
    public Color Color { get; private set; }
    public string Team { get; private set; }
    public string Name { get; private set; }
    public NetPlayer NetPlayer { get; private set; }
    public bool IsAlive { get; set; }

    public void Spawn(string playerName, string team, NetPeer peer, Vector3 spawnPoint, Color playerColor)
    {
        Name = playerName;
        Peer = peer;
        Team = team;
        Body = gameObject;
        transform.position = spawnPoint;
        Health = 100;
        Body.name = playerName;
        Body.tag = "Player";
        Color = playerColor;
        NetPlayer = gameObject.GetComponent<NetPlayer>();
        IsAlive = true;
    }

    public void Dispose()
    {
        Peer.Disconnect();
        Destroy(gameObject);
    }
}