using System.Linq;
using Basic;
using ENet;

namespace Network.Messages
{
    public class RawMessage : IMessage
    {
        public byte[] Data { get; }
        public byte MessageCode { get; }
        public bool isBroadcast { get; }
        public uint PeerID { get; set; }

        public RawMessage(byte messageType, byte[] rawData, bool isBroadCast, uint peerIDToSend)
        {
            MessageCode = messageType;
            Data = rawData;
            isBroadcast = isBroadCast;
            PeerID = peerIDToSend;
        }
        
        public static Packet ToPacket(RawMessage message, PacketFlags flags)
        {
            var packet = new Packet();
            byte[] buffer = {message.MessageCode};
            buffer = buffer.Concat(message.Data).ToArray();
            packet.Create(buffer, flags);
            return packet;
        }
        
        public void DoWork(NetworkInterfaces interfaces)
        {
            
        }
    }
}
