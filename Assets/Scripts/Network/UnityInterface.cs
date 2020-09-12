using System;
using Basic;
using Unity.Jobs;
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
        public TrueServer Server;

        private NetworkInterfaces _interfaces;

        public void Setup(TrueServer server)
        {
            Server = server;
            _interfaces = new NetworkInterfaces(GetComponent<Players>());
            Debug.Log("Unity Interface Started");
        }

        public void SendMessages(IMessage message) => Server.MessagesToSend.Writer.WriteAsync(message);

        private void Update()
        {
            var newMessage = Server.ReceivedMessages.Reader.TryRead(out var message);
                
            if (newMessage) message?.DoWork(_interfaces);
        }
    }
}
