using System.Linq;
using ENet;

namespace Network.Messages
{
    public class RawMessage
    {
        public byte MessageType { get; }
        public byte[] Data { get; }

        public RawMessage(byte messageType, byte[] rawData)
        {
            MessageType = messageType;
            Data = rawData;
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
