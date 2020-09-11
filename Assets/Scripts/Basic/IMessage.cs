using Network;
using Network.Messages;

namespace Basic
{
    public interface IMessage
    { 
        byte MessageCode { get; }
        bool isBroadcast { get; }
        uint PeerID { get; }

        void DoWork(NetworkInterfaces interfaces);
    }
}