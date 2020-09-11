using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Basic;
using MessagePack;
using Network.Messages;
using UnityEngine;

namespace Network
{
    public class SerializeDeserialize
    {
        public static TrueServer _server;

        public Channel<IMessage> receivedMessages;

        public SerializeDeserialize(TrueServer server)
        {
            _server = server;
            receivedMessages = Channel.CreateUnbounded<IMessage>(); 
            new Thread(DataToRawMessage).Start();
        }

        private void DataToRawMessage()
        {
            var channel = _server.ReceivedMessages;

            while (!_server._isQuitting)
            {
                var newMessage = channel.Reader.TryRead(out var message);
            
                if(newMessage && message != null) RawMessageToMessage(message);
            }
        }

        private void RawMessageToMessage(RawMessage message)
        {
            switch (message.MessageType)
            {
                case 0:
                    receivedMessages.Writer.WriteAsync(
                        MessagePackSerializer.Deserialize<ConnectionMessage>(message.Data));
                    break;
                case 1:
                    //_messages.Add(MessagePackSerializer.Deserialize<ConnectionMessage>(message.Data));
                    break;
            }
        }
    }
}
