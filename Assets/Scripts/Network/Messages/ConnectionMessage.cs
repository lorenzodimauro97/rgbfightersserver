using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public class ConnectionMessage : IMessage
    {
        [Key(0)]
        public string message { get; set; }
    }
}