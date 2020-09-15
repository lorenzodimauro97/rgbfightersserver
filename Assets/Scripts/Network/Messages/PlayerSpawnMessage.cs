using System;
using System.Collections.Generic;
using Basic;
using MessagePack;
using UnityEngine;

namespace Network.Messages
{
    [MessagePackObject]
    public class PlayerSpawnMessage : IMessage
    {
        [SerializationConstructor]
        public PlayerSpawnMessage(Vector3 localPlayerPosition, string localPlayerNickname,
            Dictionary<uint, string> players, uint peerID)
        {
            LocalPlayerPosition = localPlayerPosition;
            LocalPlayerNickname = localPlayerNickname;
            NetPlayers = players;
            IsBroadcast = true;
            PeerID = peerID;
        }

        [Key(0)] public Vector3 LocalPlayerPosition { get; }
        [Key(1)] public string LocalPlayerNickname { get; }
        [Key(2)] public Dictionary<uint, string> NetPlayers { get; }
        [Key(4)] public bool IsBroadcast { get; }
        [Key(3)] public uint PeerID { get; set; }


        public void DoWork(NetworkInterfaces interfaces)
        {
            throw new NotImplementedException();
        }
    }
}