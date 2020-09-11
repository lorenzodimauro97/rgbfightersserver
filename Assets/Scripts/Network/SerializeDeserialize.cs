using System.Collections.Generic;
using System.Linq;
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
        public TrueServer _server;

        public Channel<IMessage> receivedMessages;

        public SerializeDeserialize(TrueServer server)
        {
            _server = server;
            receivedMessages = Channel.CreateUnbounded<IMessage>(); 
            new Thread(DataToRawMessage).Start();
        }

        public static byte[] Serialize(IMessage message)
        {
            return MessagePackSerializer.Serialize(message);
        }

        public void MessageToRawMessage(IMessage message)
        {
            var rawMessage = new RawMessage(message.MessageCode, Serialize(message), message.isBroadcast, message.PeerID);
            _server.MessagesToSend.Writer.WriteAsync(rawMessage);
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
            switch (message.MessageCode)
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
