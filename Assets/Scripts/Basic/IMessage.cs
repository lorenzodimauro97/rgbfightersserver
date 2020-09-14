using Network;
using Network.Messages;

namespace Basic
{
    [MessagePack.Union(0, typeof(ConnectionMessage))]
    [MessagePack.Union(1, typeof(LoadMapMessage))]
    [MessagePack.Union(2, typeof(WaitingRoomMessage))]
    [MessagePack.Union(3, typeof(PlayerSpawnMessage))]
    [MessagePack.Union(4, typeof(PlayerPositionRotationMessage))]
    public interface IMessage
    {
        bool IsBroadcast { get; }
        uint PeerID { get; set; }

        void DoWork(NetworkInterfaces interfaces);
    }
}