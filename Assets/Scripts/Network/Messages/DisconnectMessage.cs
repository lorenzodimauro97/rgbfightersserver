using System.Collections;
using System.Collections.Generic;
using Basic;
using Network;
using UnityEngine;

public class DisconnectMessage : IMessage
{
    public bool IsBroadcast { get; }
    public uint PeerID { get; set; }

    public DisconnectMessage(bool isBroadcast, uint peerID)
    {
        IsBroadcast = isBroadcast;
        PeerID = peerID;
    }
    public void DoWork(NetworkInterfaces interfaces)
    {
        interfaces.Players.RemovePlayer(PeerID);
    }
}
