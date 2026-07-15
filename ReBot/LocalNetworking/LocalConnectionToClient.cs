using Mirror;
using System;

namespace ReBot.LocalNetworking
{
    public class LocalConnectionToClient(int connectionId) : NetworkConnectionToClient(connectionId)
    {
        public override string address => "localhost";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
            //NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
            //networkWriterPooled.WriteBytes(segment.Array, segment.Offset, segment.Count);
            //this.connectionToServer.queue.Enqueue(networkWriterPooled);
        }

        internal bool IsAlive(float timeout)
        {
            return true;
        }

        internal void DisconnectInternal()
        {
            isReady = false;

            NetworkServer.DestroyPlayerForConnection(this);
            NetworkServer.RemoveConnection(this.connectionId);
        }

        public override void Disconnect()
        {
            DisconnectInternal();
            connectionToServer.DisconnectInternal();
        }

        internal LocalConnectionToServer connectionToServer;
    }
}
