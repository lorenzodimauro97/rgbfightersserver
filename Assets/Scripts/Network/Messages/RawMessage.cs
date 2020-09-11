using System.Linq;
using ENet;

namespace Network.Messages
{
    public class RawMessage
    {
        public bool IsBroadCast { get; }
        public byte PeerIDToSend { get; }
        public byte MessageType { get; }
        public byte[] Data { get; }

        public RawMessage(byte messageType, byte[] rawData, bool isBroadCast, byte peerIDToSend)
        {
            MessageType = messageType;
            Data = rawData;
            IsBroadCast = isBroadCast;
            PeerIDToSend = peerIDToSend;
        }
        
        public static Packet ToPacket(RawMessage message, PacketFlags flags)
        {
            var packet = new Packet();
            byte[] buffer = {message.MessageType};
            buffer = buffer.Concat(message.Data).ToArray();
            packet.Create(buffer, flags);
            return packet;
        }
    }
}
