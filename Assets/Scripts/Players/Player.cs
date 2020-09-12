using System;
using ENet;
using MessagePack;
using UnityEngine;

namespace Players
{
    [MessagePackObject]
    public class SerializablePlayer
    {
        [Key(0)] public string Nickname { get; }
        [Key(1)] public uint ID { get; set; }
        [IgnoreMember] public Player Player;
        [Key(2)] public Vector3 Position { get; private set; }
        [Key(3)] public Quaternion Rotation { get; private set; }
        [Key(4)] public Quaternion HeadRotation { get; private set; }
        
        public SerializablePlayer(string nickname, Player player)
        {
            Nickname = nickname;
            Player = player;
        }

        [SerializationConstructor]
        public SerializablePlayer(string nickname, uint id)
        {
            Nickname = nickname;
            ID = id;
        }

        public void UpdatePositionRotation(Vector3 position, Quaternion rotation, Quaternion headRotation)
        {
            Player.Body.transform.position = position;
            Player.Body.transform.rotation = rotation;
            Player.head.transform.rotation = headRotation;
        }
    }
    
    public class Player : MonoBehaviour
    {
        public GameObject Body { get; set; }

        public GameObject head;

        public SerializablePlayer SerializablePlayer;

        private void Start()
        {
            Body = gameObject;
            SerializablePlayer = new SerializablePlayer(null, null);
        }

        public void SetSerializablePlayer(SerializablePlayer serializablePlayer, uint id)
        {
            SerializablePlayer = serializablePlayer;
            SerializablePlayer.Player = this;
            serializablePlayer.ID = id;
            gameObject.name = serializablePlayer.Nickname;

            Debug.Log($"Player {serializablePlayer.Nickname} Connected!");
        }
    }
}