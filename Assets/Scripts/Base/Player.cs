using System;
using LiteNetLib;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class Player : MonoBehaviour
{
    public int Health { get; private set; }
    public NetPeer Peer { get; private set; }
    public GameObject Body { get; set; }
    public Color Color { get; private set; }
    public string Team { get; private set; }
    public string Name { get; private set; }
    public NetPlayer netPlayer;

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
        netPlayer = gameObject.GetComponent<NetPlayer>();
    }

    public void Dispose()
    {
        Peer.Disconnect();
        Destroy(gameObject);
    }
}