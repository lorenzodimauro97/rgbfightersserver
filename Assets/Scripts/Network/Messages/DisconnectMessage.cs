using Basic;
using Network;

public class DisconnectMessage : IMessage
{
    public DisconnectMessage(bool isBroadcast, uint peerID)
    {
        IsBroadcast = isBroadcast;
        PeerID = peerID;
    }

    public bool IsBroadcast { get; }
    public uint PeerID { get; set; }

    public void DoWork(NetworkInterfaces interfaces)
    {
        interfaces.Players.RemovePlayer(PeerID);
    }
}