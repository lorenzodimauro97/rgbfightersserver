using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using ENet;
using Network.Messages;
using UnityEngine;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace Network
{ 
    public class TrueServer
    {
        public Channel<RawMessage> ReceivedMessages;
        public Channel<RawMessage> MessagesToSend;
        
        private Host _server;
        private Dictionary<uint, Peer> _peers;
        public bool _isQuitting = false;
        private Event _netEvent;
        
        public void Start()
        {
            Library.Initialize();
            Application.wantsToQuit += () => _isQuitting = true;

            _server = new Host();

            var address = new Address {Port = (ushort) ConfigParser.GetValueInt("ipPort")};

            _peers = new Dictionary<uint, Peer>();
            _server.Create(address, ConfigParser.GetValueInt("maximumPlayers"));
            
            Debug.Log($"Starting server at port {address.Port}");

            ReceivedMessages = Channel.CreateUnbounded<RawMessage>();
            MessagesToSend = Channel.CreateUnbounded<RawMessage>();

            new Thread(FetchSendNetEvent).Start();
        }

        private void FetchSendNetEvent()
        {
            Debug.Log("Starting Message Fetching / Sending...");
            
            while (!_isQuitting)
            {
                SendMessage();
                _server.Flush();
                
                _server.CheckEvents(out _netEvent);
                _server.Service(0, out _netEvent);

                switch (_netEvent.Type) {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Debug.Log("Client connected - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            _peers.Add(_netEvent.Peer.ID, _netEvent.Peer);
                            break;

                        case EventType.Disconnect:
                            Debug.Log("Client disconnected - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            _peers.Remove(_netEvent.Peer.ID);
                            break;

                        case EventType.Timeout:
                            Debug.Log("Client timeout - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            _peers.Remove(_netEvent.Peer.ID);
                            break;

                        case EventType.Receive:
                            Debug.Log("Packet received from - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP + ", Channel ID: " + _netEvent.ChannelID + ", Data length: " + _netEvent.Packet.Length);
                            AddReceivedPacketToChannel(_netEvent);
                            _netEvent.Packet.Dispose();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                }
            }
            Library.Deinitialize();
        }

        private void AddReceivedPacketToChannel(Event netEvent)
        {
            var buffer = new byte[_netEvent.Packet.Length];

            netEvent.Packet.CopyTo(buffer);
            
            var rawMessage = new RawMessage(buffer[0], buffer.Skip(1).ToArray(), false, 0);

            ReceivedMessages.Writer.WriteAsync(rawMessage);
        }

        private void SendMessage()
        {
            var newMessage = MessagesToSend.Reader.TryRead(out var message);
            
            if (!newMessage || message == null) return;
            
            var packet = RawMessage.ToPacket(message, PacketFlags.Reliable);

            if(message.IsBroadCast) _server.Broadcast(0, ref packet);
            else _peers[message.PeerIDToSend].Send(0, ref packet);
        }
    }

    public class Server : MonoBehaviour
    {
        private TrueServer _server;
        private SerializeDeserialize _serializer;
        private void Start()
        {
            _server = new TrueServer();
            _server.Start();
            _serializer = new SerializeDeserialize(_server);
        }
    }
}
