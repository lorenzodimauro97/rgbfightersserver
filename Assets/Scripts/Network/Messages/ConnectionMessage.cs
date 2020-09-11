using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public abstract class ConnectionMessage : IMessage
    {
        [Key(0)]
        public string message { get; set; }
        [Key(1)]
        public byte messageCode { get; set; }
    }
}