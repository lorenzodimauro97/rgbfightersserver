using Basic;
using ENet;
using MessagePack;

namespace Network.Messages
{
    public class RawMessage
    {
        public byte[] Data { get; }
        public bool IsBroadcast { get; }
        public RawMessage(byte[] data, bool isBroadCast)
        {
            Data = data;
            IsBroadcast = isBroadCast;
        }
        
        public RawMessage(IMessage data)
        {
            Data = MessagePackSerializer.Serialize(data);
            IsBroadcast = data.IsBroadcast;
        }
        
        public Packet Packet(PacketFlags flags)
        {
            var packet = new Packet();
            packet.Create(Data, flags);
            return packet;
        }

        public IMessage Message()
        {
            return MessagePackSerializer.Deserialize<IMessage>(Data);
        }
    }
}
