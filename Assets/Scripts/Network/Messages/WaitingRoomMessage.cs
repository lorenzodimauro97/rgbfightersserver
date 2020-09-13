using Basic;
using MessagePack;

namespace Network.Messages
{
    [MessagePackObject]
    public class WaitingRoomMessage : IMessage
    { 
        [Key(0)] public int NumberOfPlayersOnline { get; }
        [Key(1)] public int MinimumPlayersRequired { get; }
        [Key(2)] public string ServerIntro { get; }
        [Key(3)] public bool IsBroadcast { get; }
        [Key(4)] public uint PeerID { get; set; }

        [SerializationConstructor]
        public WaitingRoomMessage(int numberOfPlayersOnline, int minimumPlayersRequired, string serverIntro, bool isBroadcast)
        {
            NumberOfPlayersOnline = numberOfPlayersOnline;
            MinimumPlayersRequired = minimumPlayersRequired;
            ServerIntro = serverIntro;
            IsBroadcast = isBroadcast;
        }

        public void DoWork(NetworkInterfaces interfaces)
        {
            throw new System.NotImplementedException();
        }
    }
}
