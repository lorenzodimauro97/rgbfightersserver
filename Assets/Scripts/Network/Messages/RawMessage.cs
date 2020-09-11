namespace Network.Messages
{
    public class RawMessage
    {
        public int MessageType { get; }
        public byte[] Data { get; }

        public RawMessage(int messageType, byte[] rawData)
        {
            MessageType = messageType;
            Data = rawData;
        }
    }
}
