using MessagePack;
using Network;
using Network.Messages;

namespace Basic
{
    [Union(0, typeof(ConnectionMessage))]
    [Union(1, typeof(LoadMapMessage))]
    [Union(2, typeof(WaitingRoomMessage))]
    [Union(3, typeof(PlayerSpawnMessage))]
    [Union(4, typeof(PlayerPositionRotationMessage))]
    public interface IMessage
    {
        bool IsBroadcast { get; }
        uint PeerID { get; set; }

        void DoWork(NetworkInterfaces interfaces);
    }
}