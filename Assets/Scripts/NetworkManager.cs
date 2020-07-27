using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class NetworkManager : MonoBehaviour, INetEventListener
{
    public int connectedPeerLimit;
    public int connectedPeers;
    public MessageHandler messageHandler;

    public NetManager netManager;
    public NetworkFPSManager networkFps;
    public NetworkMapManager networkMap;

    public NetworkPlayers networkPlayer;
    public NetDataWriter writer;

    public void OnConnectionRequest(ConnectionRequest request)
    {
        var key = request.Data.GetString();

        if (connectedPeers < connectedPeerLimit &&
            key == $"rgbfighters:{Application.version}")
            request.Accept();

        else if (connectedPeers >= connectedPeerLimit)
            request.Reject(Encoding.ASCII.GetBytes("SServerFull"));

        else if (key != $"rgbfighters:{Application.version}")
            request.Reject(Encoding.ASCII.GetBytes("SServerClientMismatch"));
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.Log(2);
    }


    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        //Debug.Log(3);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        var message = reader.GetString();

        //Debug.Log("Messaggio Ricevuto: " + message);
        messageHandler.HandleIncomingMessage(message, peer);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
        Debug.LogError("Messaggio non autorizzato!");
    }


    public void OnPeerConnected(NetPeer peer)
    {
    }


    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var disconnectedPlayer = networkPlayer.FindPlayer(peer);

        if (!disconnectedPlayer) return;

        SendPeerDisconnectionToClients(disconnectedPlayer);
        disconnectedPlayer.Dispose();
        networkPlayer.players.RemoveAll(x => x.GetPeer() == peer);
    }

    private void Start()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("it");
        if (GameObject.FindGameObjectsWithTag("NetworkManager").Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        messageHandler = GetComponent<MessageHandler>();
        networkPlayer = GetComponent<NetworkPlayers>();
        networkMap = GetComponent<NetworkMapManager>();
        networkFps = GetComponent<NetworkFPSManager>();
        DontDestroyOnLoad(this);
        CreateServer();
    }

    private bool SetupNetManager()
    {
        writer = new NetDataWriter();
        netManager = new NetManager(this)
        {
            UnconnectedMessagesEnabled = true,
            BroadcastReceiveEnabled = true,
            UpdateTime = 10,
            MaxConnectAttempts = 3
        };
        return true;
    }

    private void CreateServer()
    {
        if (SetupNetManager()) netManager.Start(1337);
        if (!netManager.IsRunning)
        {
            Debug.LogError("Errore! Il server non si è avviato!");
            Application.Quit();
        }

        Debug.Log($"Server avviato su porta {netManager.LocalPort}");
        networkMap.StartMapManager();
    }

    public static void DisconnectClient(string reason, NetPeer peer)
    {
        peer.Disconnect();
    }

    private void Update()
    {
        if (netManager.IsRunning) netManager.PollEvents();
    }

    public void SendChatMessage(string message)
    {
        SendMessageToClient(message);
    }

    public void SendMessageToClient(string message)
    {
        MessageHandler.HandleSendingToAll(writer, message, netManager);
    }

    public void SendMessageToClient(string message, NetPeer peer)
    {
        MessageHandler.HandleSendingMessage(writer, message, peer);
    }


    private void SendPeerDisconnectionToClients(Player player)
    {
        SendMessageToClient($"PlayerDisconnected@{player.GetPeerId()}");

        SendChatMessage($"ChatMessage@Server:{player.Name} Si è disconnesso!");
    }

    private void OnApplicationQuit()
    {
        netManager.Stop();
    }
}