using System.Collections.Generic;
using System.Threading;
using Basic;
using MessagePack;
using Network.Messages;
using UnityEngine;

namespace Network
{
    public class SerializeDeserialize
    {
        private static TrueServer _server;

        private List<IMessage> _messages;

        public SerializeDeserialize(TrueServer server)
        {
            _server = server;
            _messages = new List<IMessage>();
            new Thread(DataToRawMessage).Start();
        }

        private void DataToRawMessage()
        {
            var channel = _server.ReceivedMessages;

            RawMessage message;

            while (!_server._isQuitting)
            { 
                var newMessage = channel.Reader.TryRead(out message);
            
                if(newMessage) Debug.Log(message.MessageType);
                if(message != null) RawMessageToMessage(message);
            }
        }

        private void RawMessageToMessage(RawMessage message)
        {
            switch (message.MessageType)
            {
                case 0:
                    _messages.Add(MessagePackSerializer.Deserialize<ConnectionMessage>(message.Data));
                    break;
                case 1:
                    _messages.Add(MessagePackSerializer.Deserialize<ConnectionMessage>(message.Data));
                    break;
            }

            ConnectionMessage conmes = (ConnectionMessage) _messages[0];
            Debug.Log(conmes.message);
        }
    }
}
