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
        [IgnoreMember] public Player Player;
        [Key(1)] public Vector3 Position { get; private set; }
        [Key(2)] public Quaternion Rotation { get; private set; }
        [Key(3)] public Quaternion HeadRotation { get; private set; }
        
        public SerializablePlayer(string nickname, Player player)
        {
            Nickname = nickname;
            Player = player;
        }

        [SerializationConstructor]
        public SerializablePlayer(string nickname)
        {
            Nickname = nickname;
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
            SerializablePlayer = new SerializablePlayer(null);
        }

        public void SetSerializablePlayer(SerializablePlayer serializablePlayer)
        {
            SerializablePlayer = serializablePlayer;
            SerializablePlayer.Player = this;
            
            Debug.Log($"Hello! I'm {serializablePlayer.Nickname}!");
        }
    }
}