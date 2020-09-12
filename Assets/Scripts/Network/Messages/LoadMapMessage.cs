using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public class LoadMapMessage : IMessage
    {
        [Key(0)] public int MapIndex { get; }
        [Key(1)] public string MapDownloadLink { get; }
        [Key(2)] public string MapHash { get; }
        [Key(3)] public bool IsBroadcast { get; set; }
        [Key(4)] public uint PeerID { get; }

        [SerializationConstructor]
        public LoadMapMessage(int mapIndex, string mapDownloadLink, string mapHash, bool isBroadcast, uint peerID)
        {
            MapIndex = mapIndex;
            MapDownloadLink = mapDownloadLink;
            MapHash = mapHash;
            IsBroadcast = isBroadcast;
            PeerID = peerID;
        }
        public void DoWork(NetworkInterfaces interfaces)
        {
            throw new System.NotImplementedException();
        }
    }
}
