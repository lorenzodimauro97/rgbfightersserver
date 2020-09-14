using System;
using System.Threading;
using Basic;
using Unity.Jobs;
using UnityEngine;

namespace Network
{
    public class NetworkInterfaces
    {
        public Players Players { get; }
        public GameplayManager GameplayManager { get; }

        public NetworkInterfaces(Players players, GameplayManager manager)
        {
            Players = players;
            GameplayManager = manager;
        }
    }
    public class UnityInterface : MonoBehaviour
    {
        public TrueServer Server;

        public NetworkInterfaces _interfaces;

        public void Setup(TrueServer server)
        {
            Server = server;
            _interfaces = new NetworkInterfaces(GetComponent<Players>(), GetComponent<GameplayManager>());
            _interfaces.GameplayManager.StartMapManager();
            Debug.Log("Unity Interface Started");
        }

        public void SendMessages(IMessage message) => Server.MessagesToSend.Writer.WriteAsync(message);

        private void Update()
        {
            if(Server == null) return;
            
            var newMessage = Server.ReceivedMessages.Reader.TryRead(out var message);

            if (!newMessage) return;
            
            message?.DoWork(_interfaces);
            Debug.Log($"Received {message}");
        }
    }
}
