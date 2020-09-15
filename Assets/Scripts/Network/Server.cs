using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Basic;
using ENet;
using Network.Messages;
using UnityEngine;
using UnityEngine.Serialization;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace Network
{
    public class TrueServer
    {
        public bool _isQuitting;
        private Dictionary<uint, Peer> _peers;

        private Host _server;
        public Channel<IMessage> MessagesToSend;
        public int peerLimit;
        public Channel<IMessage> ReceivedMessages;

        public void Start()
        {
            Library.Initialize();
            Application.wantsToQuit += () => _isQuitting = true;

            _server = new Host();

            var address = new Address {Port = (ushort) ConfigParser.GetValueInt("ipPort")};
            peerLimit = ConfigParser.GetValueInt("maximumPlayers");

            _peers = new Dictionary<uint, Peer>();
            _server.Create(address, ConfigParser.GetValueInt("maximumPlayers"));

            Debug.Log($"Starting server at port {address.Port}");

            ReceivedMessages = Channel.CreateUnbounded<IMessage>();
            MessagesToSend = Channel.CreateUnbounded<IMessage>();

            new Thread(FetchSendNetEvent).Start();
        }

        private async void FetchSendNetEvent()
        {
            Debug.Log("Starting Message Fetching / Sending...");

            while (!_isQuitting)
            {
                SendMessage();
                _server.Flush();

                _server.CheckEvents(out var netEvent);
                _server.Service(0, out netEvent);

                switch (netEvent.Type)
                {
                    case EventType.None:
                        break;

                    case EventType.Connect:
                        _peers.Add(netEvent.Peer.ID, netEvent.Peer);
                        break;

                    case EventType.Disconnect:
                        Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        _peers.Remove(netEvent.Peer.ID);
                        ReceivedMessages.Writer.WriteAsync(new DisconnectMessage(false, netEvent.Peer.ID));
                        break;

                    case EventType.Timeout:
                        Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        _peers.Remove(netEvent.Peer.ID);
                        ReceivedMessages.Writer.WriteAsync(new DisconnectMessage(false, netEvent.Peer.ID));
                        break;

                    case EventType.Receive:
                        Debug.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP +
                                  ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                        AddReceivedPacketToChannel(netEvent);
                        netEvent.Packet.Dispose();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Task.Delay(3);
            }

            Library.Deinitialize();
        }

        private void AddReceivedPacketToChannel(Event netEvent)
        {
            var buffer = new byte[netEvent.Packet.Length];

            netEvent.Packet.CopyTo(buffer);

            var rawMessage = new RawMessage(buffer, false);

            var message = rawMessage.Message();

            message.PeerID = netEvent.Peer.ID;

            ReceivedMessages.Writer.WriteAsync(message);
        }

        private void SendMessage()
        {
            var newMessage = MessagesToSend.Reader.TryRead(out var message);

            if (!newMessage || message == null) return;

            var packet = new RawMessage(message).Packet(PacketFlags.Reliable);

            if (message.IsBroadcast) _server.Broadcast(0, ref packet);
            else _peers[message.PeerID].Send(0, ref packet);
        }
    }

    public class Server : MonoBehaviour
    {
        public UnityInterface @interface;
        public Host _server;
        public bool isQuitting;
        private Dictionary<uint, Peer> _peers;
        
        public int peerLimit;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            StartServer();
            @interface = GetComponent<UnityInterface>();
            @interface.Setup(this);
        }
        
        public void StartServer()
        {
            Library.Initialize();

            _server = new Host();

            var address = new Address {Port = (ushort) ConfigParser.GetValueInt("ipPort")};
            peerLimit = ConfigParser.GetValueInt("maximumPlayers");

            _peers = new Dictionary<uint, Peer>();
            _server.Create(address, peerLimit);

            Debug.Log($"Starting server at port {address.Port}");
        }

        private void FixedUpdate()
        {
            _server.Flush();

            if (_server.CheckEvents(out var netEvent) <= 0)
            {
                if (_server.Service(0, out netEvent) <= 0)
                    return;
            }

            switch (netEvent.Type)
            {
                case EventType.None:
                    break;

                case EventType.Connect:
                    _peers.Add(netEvent.Peer.ID, netEvent.Peer); 
                    break;

                case EventType.Disconnect: 
                    Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP); 
                    _peers.Remove(netEvent.Peer.ID); 
                    new DisconnectMessage(false, netEvent.Peer.ID).DoWork(@interface.Interfaces); 
                    break;

                case EventType.Timeout: 
                    Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP); 
                    _peers.Remove(netEvent.Peer.ID); 
                    new DisconnectMessage(false, netEvent.Peer.ID).DoWork(@interface.Interfaces);
                    break;

                case EventType.Receive: 
                    Debug.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + 
                              ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                    ReceivePacket(netEvent); 
                    netEvent.Packet.Dispose(); 
                    break;
                    default:
                        throw new ArgumentOutOfRangeException();
            }
        }

        private void ReceivePacket(Event netEvent)
        {
            var buffer = new byte[netEvent.Packet.Length];

            netEvent.Packet.CopyTo(buffer);

            var rawMessage = new RawMessage(buffer, false);

            var message = rawMessage.Message();

            message.PeerID = netEvent.Peer.ID;
            
            message.DoWork(@interface.Interfaces);
        }

        public void SendMessage(IMessage message)
        {
            var packet = new RawMessage(message).Packet(PacketFlags.Reliable);

            if (message.IsBroadcast) _server.Broadcast(0, ref packet);
            else _peers[message.PeerID].Send(0, ref packet);
        }
    }
}