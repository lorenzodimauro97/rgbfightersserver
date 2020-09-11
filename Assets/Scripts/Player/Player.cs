using System;
using ENet;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class Player : MonoBehaviour
    {
        public float Health { get; set; }
        private Peer Peer { get; set; }
        public GameObject Body { get; set; }
        public Color Color { get; private set; }
        public string Team { get; private set; }
        public string Name { get; private set; }
        public bool IsAlive { get; private set; }
        public string GunIndex { get; set; }

        public GameObject head;

        public void Spawn(string playerName, string team, Peer peer, Vector3 spawnPoint, Color playerColor)
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
            IsAlive = true;
            GunIndex = "0";
        }

        public void SetAlive(bool status)
        {
            IsAlive = status;
            if (status) Health = 100;
        }

        public uint GetPeerId()
        {
            return Peer.ID;
        }

        public void MovePlayer(Vector3 position, Quaternion rotation, Quaternion headRotation )
        {

            transform.position = position;
            transform.rotation = rotation;
            head.transform.rotation = headRotation;

            if (position.y > -200 || !IsAlive) return;
            //StartCoroutine(_networkManager.networkPlayer.KillPlayer(_player));
        }
    }
}