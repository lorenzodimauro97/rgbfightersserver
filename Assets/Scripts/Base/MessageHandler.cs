using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{
    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    public void HandleIncomingMessage(string data, NetPeer peer)
    {
        var dataArray = DataFormatter.StringToArray(data, "@"); //Data[0] è sempre il comando ricevuto dal client
        //Debug.Log(data);
        switch (dataArray[0])
        {
            case "PlayerMapLoaded":
                _networkManager.networkPlayer.SpawnPlayer(dataArray, peer);
                break;
            case "PlayerConnected":
                _networkManager.networkPlayer.StartPlayer(dataArray, peer);
                break;
            case "PlayerPosition":
                _networkManager.networkPlayer.MovePlayer(dataArray, peer);
                break;
            case "ChatMessage":
                _networkManager.SendChatMessage(data);
                break;
            case "GunShoot":
                _networkManager.networkFps.ParseShootingData(dataArray, peer);
                break;
            case "GunEntityShoot":
                _networkManager.networkFps.ParseEntityShootData(dataArray);
                break;
            case "GunChange":
                _networkManager.networkPlayer.ChangePlayerGunIndex(dataArray[1], peer);
                break;
        }
    }

    public static void HandleSendingMessage(NetDataWriter writer, string data, NetPeer peer)
    {
        writer.Reset();
        writer.Put(data);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        //Debug.Log("Messaggio inviato:" + data);
    }

    public static void HandleSendingToAll(NetDataWriter writer, string data, NetManager manager)
    {
        writer.Reset();
        writer.Put(data);
        manager.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }
}