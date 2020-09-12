using Network;
using Network.Messages;

namespace Basic
{
    [MessagePack.Union(0, typeof(ConnectionMessage))]
    public interface IMessage
    { 
        byte MessageCode { get; }
        bool IsBroadcast { get; }
        uint PeerID { get; }

        void DoWork(NetworkInterfaces interfaces);
    }
}