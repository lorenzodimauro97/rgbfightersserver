using Network;
using Network.Messages;

namespace Basic
{
    [MessagePack.Union(0, typeof(ConnectionMessage))]
    [MessagePack.Union(1, typeof(LoadMapMessage))]
    public interface IMessage
    {
        bool IsBroadcast { get; }
        uint PeerID { get; }

        void DoWork(NetworkInterfaces interfaces);
    }
}