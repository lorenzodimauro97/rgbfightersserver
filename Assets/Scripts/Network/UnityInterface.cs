using System;
using System.Collections;
using UnityEngine;

namespace Network
{
    public class NetworkInterfaces
    {
        public Players Players { get; }

        public NetworkInterfaces(Players players)
        {
            Players = players;
        }
    }
    public class UnityInterface : MonoBehaviour
    {
        public SerializeDeserialize serializer;

        private NetworkInterfaces _interfaces;

        private void Start()
        {
            serializer = GetComponent<Server>()._serializer;
            _interfaces = new NetworkInterfaces(GetComponent<Players>());
            StartCoroutine(ReadMessages());
        }

        private IEnumerator ReadMessages()
        {
            while (!serializer._server._isQuitting)
            {
                var newMessage = serializer.receivedMessages.Reader.TryRead(out var message);

                if (newMessage) message?.DoWork(_interfaces);
            }
            
            yield break;
        }
    }
}
