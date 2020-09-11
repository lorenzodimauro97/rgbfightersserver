using System;
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
        public Channel<RawMessage> Channel;
        
        private Host _server;
        private bool _isQuitting = false;
        private Event _netEvent;
        
        public void Start()
        {
            Library.Initialize();
            Application.wantsToQuit += () => _isQuitting = true;

            _server = new Host();

            var address = new Address {Port = (ushort) ConfigParser.GetValueInt("ipPort")};

            _server.Create(address, ConfigParser.GetValueInt("maximumPlayers"));
            
            Debug.Log($"Starting server at port {address.Port}");

            Channel = System.Threading.Channels.Channel.CreateUnbounded<RawMessage>();

            new Thread(FetchNetEvent).Start();
        }

        private void FetchNetEvent()
        {
            Debug.Log("Starting Message Fetching / Sending...");
            
            while (!_isQuitting)
            {
                _server.Flush();
                
                _server.CheckEvents(out _netEvent);
                _server.Service(0, out _netEvent);

                switch (_netEvent.Type) {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Debug.Log("Client connected - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            break;

                        case EventType.Disconnect:
                            Debug.Log("Client disconnected - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            break;

                        case EventType.Timeout:
                            Debug.Log("Client timeout - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP);
                            break;

                        case EventType.Receive:
                            Debug.Log("Packet received from - ID: " + _netEvent.Peer.ID + ", IP: " + _netEvent.Peer.IP + ", Channel ID: " + _netEvent.ChannelID + ", Data length: " + _netEvent.Packet.Length);
                            _netEvent.Packet.Dispose();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                }
            }
            Library.Deinitialize();
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
        }
    }
}
