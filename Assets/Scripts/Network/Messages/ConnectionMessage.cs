using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public abstract class ConnectionMessage : IMessage
    {
        [Key(0)] public Player.Player Player;

        [Key(1)] public byte MessageCode { get; set; }
        [Key(2)] public bool isBroadcast { get; set; }
        [Key(3)] public uint PeerID { get; set; }

        public void DoWork(NetworkInterfaces interfaces)
        {
            interfaces.Players.AddPlayer(Player);
        }
    }
}