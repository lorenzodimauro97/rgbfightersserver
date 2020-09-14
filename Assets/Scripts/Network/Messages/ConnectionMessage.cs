using Basic;
using Players;
using MessagePack;
using UnityEngine;

namespace Network.Messages
{
    [MessagePackObject]
    public class ConnectionMessage : IMessage
    {
        [Key(0)] public string PlayerNickname { get; }
        [Key(1)] public bool IsBroadcast { get; set; }
        [Key(2)] public uint PeerID { get; set; }


        [SerializationConstructor]
        public ConnectionMessage(string player)
        {
            PlayerNickname = player;
            IsBroadcast = false;
        }

        public void DoWork(NetworkInterfaces interfaces)
        {
            interfaces.Players.AddPlayer(PeerID, PlayerNickname);

            IsBroadcast = true;
        }
    }
}