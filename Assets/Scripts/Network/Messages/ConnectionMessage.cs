using Basic;
using Players;
using MessagePack;
using UnityEngine;

namespace Network.Messages
{
    [MessagePackObject]
    public class ConnectionMessage : IMessage
    {
        [Key(0)] public SerializablePlayer Player { get; }
        [Key(1)] public byte MessageCode { get; }
        [Key(2)] public bool IsBroadcast { get; }
        [Key(3)] public uint PeerID { get; }


        [SerializationConstructor]
        public ConnectionMessage(SerializablePlayer player)
        {
            Player = player;
            MessageCode = 0;
            IsBroadcast = false;
        }

        public void DoWork(NetworkInterfaces interfaces)
        {
            interfaces.Players.AddPlayer(PeerID, Player);
        }
    }
}