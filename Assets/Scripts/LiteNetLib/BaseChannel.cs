using System.Collections.Generic;

namespace LiteNetLib
{
    internal abstract class BaseChannel
    {
        protected readonly Queue<NetPacket> OutgoingQueue;
        protected readonly NetPeer Peer;
        public BaseChannel Next;

        protected BaseChannel(NetPeer peer)
        {
            Peer = peer;
            OutgoingQueue = new Queue<NetPacket>(64);
        }

        public int PacketsInQueue => OutgoingQueue.Count;

        public void AddToQueue(NetPacket packet)
        {
            lock (OutgoingQueue)
            {
                OutgoingQueue.Enqueue(packet);
            }
        }

        public abstract void SendNextPackets();
        public abstract bool ProcessPacket(NetPacket packet);
    }
}