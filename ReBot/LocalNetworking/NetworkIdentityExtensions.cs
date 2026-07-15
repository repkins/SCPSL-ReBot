using Mirror;

namespace ReBot.LocalNetworking
{
    internal static class NetworkIdentityExtensions
    {
        internal static void RemoveObserver(this NetworkIdentity identity, NetworkConnection conn)
        {
            identity.observers.Remove(conn.connectionId);
        }
    }
}
