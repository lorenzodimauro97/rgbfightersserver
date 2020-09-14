using Basic;
using MessagePack;
using UnityEngine;

namespace Network.Messages
{
    [MessagePackObject]
    public class PlayerPositionRotationMessage : IMessage
    {
        [Key(0)] public Vector3 Position { get; private set; }
        [Key(1)] public Quaternion Rotation { get; private set; }
        [Key(2)] public Quaternion HeadRotation { get; private set; }
        [Key(3)] public bool IsBroadcast { get; set; }
        [Key(4)] public uint PeerID { get; set; }

        [SerializationConstructor]
        public PlayerPositionRotationMessage(Vector3 position, Quaternion rotation, Quaternion headRotation)
        {
            Position = position;
            Rotation = rotation;
            HeadRotation = headRotation;
        }

        public void SetPositionRotation(Vector3 position, Quaternion rotation, Quaternion headRotation)
        {
            Position = position;
            Rotation = rotation;
            HeadRotation = headRotation;
        }
        public void DoWork(NetworkInterfaces interfaces)
        {
            interfaces.Players.SetPlayerPositionRotation(PeerID, Position, Rotation, HeadRotation, this);
        }
    }
}