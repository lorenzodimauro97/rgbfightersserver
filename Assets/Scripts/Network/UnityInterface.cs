using Basic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network
{
    public class NetworkInterfaces
    {
        public NetworkInterfaces(Players players, GameplayManager manager)
        {
            Players = players;
            GameplayManager = manager;
        }

        public Players Players { get; }
        public GameplayManager GameplayManager { get; }
    }

    public class UnityInterface : MonoBehaviour
    {
        public NetworkInterfaces Interfaces;
        public Server server;

        public void Setup(Server server)
        {
            this.server = server;
            Interfaces = new NetworkInterfaces(GetComponent<Players>(), GetComponent<GameplayManager>());
            Interfaces.GameplayManager.StartMapManager();
            Debug.Log("Unity Interface Started");
        }

        public void SendMessages(IMessage message)
        {
            server.SendMessage(message);
        }
    }
}