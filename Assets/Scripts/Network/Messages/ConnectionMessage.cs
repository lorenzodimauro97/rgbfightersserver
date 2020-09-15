using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public class ConnectionMessage : IMessage
    {
        [SerializationConstructor]
        public ConnectionMessage(string player)
        {
            PlayerNickname = player;
            IsBroadcast = false;
        }

        [Key(0)] public string PlayerNickname { get; }
        [Key(1)] public bool IsBroadcast { get; set; }
        [Key(2)] public uint PeerID { get; set; }

        public void DoWork(NetworkInterfaces interfaces)
        {
            interfaces.Players.AddPlayer(PeerID, PlayerNickname);

            IsBroadcast = true;
        }
    }
}