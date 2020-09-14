using System;
using System.Collections.Generic;
using Basic;
using MessagePack;
using Players;
using UnityEngine;

namespace Network.Messages
{
    [MessagePackObject]
    public class PlayerSpawnMessage : IMessage
    { 
        [Key(0)] public Vector3 LocalPlayerPosition { get; }
        [Key(1)] public string LocalPlayerNickname { get; }
        [Key(2)]public Dictionary<uint, string> NetPlayers { get; }
        [Key(4)] public bool IsBroadcast { get; }
        [Key(3)] public uint PeerID { get; set; }

        [SerializationConstructor]
        public PlayerSpawnMessage(Vector3 localPlayerPosition, string localPlayerNickname, Dictionary<uint, string> players, uint peerID)
        {
            LocalPlayerPosition = localPlayerPosition;
            LocalPlayerNickname = localPlayerNickname;
            NetPlayers = players;
            IsBroadcast = true;
            PeerID = peerID;
        }

        
        public void DoWork(NetworkInterfaces interfaces)
        {
            throw new NotImplementedException();
        }
    }
}