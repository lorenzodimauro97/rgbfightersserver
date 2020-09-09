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
    public MessageHandler messageHandler;
    public NetworkFPSManager networkFps;
    public NetworkMapManager networkMap;
    public NetworkEntityManager networkEntity;
    public NetworkLeaderboard networkLeaderboard;

    public NetworkPlayers networkPlayer;

    public NetManager netManager;
    public NetDataWriter writer;

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
        networkEntity = GetComponent<NetworkEntityManager>();
        networkLeaderboard = GetComponent<NetworkLeaderboard>();
        DontDestroyOnLoad(this);
        CreateServer();
    }

    private void Update()
    {
        if (netManager.IsRunning) netManager.PollEvents();
    }

    private void OnApplicationQuit()
    {
        netManager.Stop();
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        var key = request.Data.GetString();

        if (netManager.ConnectedPeersCount < connectedPeerLimit &&
            key == $"rgbfighters:{Application.version}")
            request.Accept();

        else if (netManager.ConnectedPeersCount >= connectedPeerLimit)
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
        var disconnectedPlayer = networkPlayer.RemovePlayer(peer);

        if (!disconnectedPlayer) return;

        SendPeerDisconnectionToClients(disconnectedPlayer);
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

        networkLeaderboard.SendLeaderBoard();

        SendChatMessage($"ChatMessage@Server:{player.Name} Si è disconnesso!");
    }
}